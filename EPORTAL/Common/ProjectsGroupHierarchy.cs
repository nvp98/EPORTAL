using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;
using System.Linq;

namespace EPORTAL.Common
{
    /// <summary>
    /// Group voi N-level hierarchy (ParentIDGroup). Vi EDMX auto-generated khong co
    /// column ParentIDGroup, fetch + write qua raw SQL ADO.NET. Migration:
    /// App_Data/migrations/v360-all.sql SECTION 2-4
    /// </summary>
    public class ProjectGroupNode
    {
        public int IDGroup { get; set; }
        public string GroupName { get; set; }
        public int? ParentIDGroup { get; set; }
        public bool Active { get; set; }
        // Cap do trong cay (0 = root). Set boi BuildTree, dung cho indent UI.
        public int Depth { get; set; }
        // Sap xep sibling: nho hon = len truoc. Default 0 -> fallback theo GroupName.
        public int SortOrder { get; set; }
        // So project truc tiep thuoc group nay (khong tinh sub-groups). Optional, do admin fill.
        public int ProjectCount { get; set; }
        public List<ProjectGroupNode> Children { get; set; }

        public ProjectGroupNode()
        {
            Children = new List<ProjectGroupNode>();
        }
    }

    public static class ProjectsGroupHierarchy
    {
        private static string GetSqlConnString()
        {
            var entry = ConfigurationManager.ConnectionStrings["EPORTALEntities"];
            if (entry == null) throw new InvalidOperationException("Connection 'EPORTALEntities' missing");
            var raw = entry.ConnectionString;
            if (raw.IndexOf("metadata=", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                try { return new EntityConnectionStringBuilder(raw).ProviderConnectionString; }
                catch { }
            }
            return raw;
        }

        // Detect col ParentIDGroup ton tai khong - migration co the chua chay.
        private static bool HasParentColumn(SqlConnection conn)
        {
            return HasColumn(conn, "ParentIDGroup");
        }

        private static bool HasColumn(SqlConnection conn, string colName)
        {
            using (var chk = new SqlCommand(
                "SELECT 1 FROM sys.columns WHERE Name=@n AND Object_ID=Object_ID('dbo.ProjectsGroup')", conn))
            {
                chk.Parameters.AddWithValue("@n", colName);
                return chk.ExecuteScalar() != null;
            }
        }

        /// <summary>
        /// Fetch tat ca groups user co quyen + build cay (N-cap).
        /// </summary>
        public static List<ProjectGroupNode> GetHierarchy(IEnumerable<int> allowedGroupIds)
        {
            var allowed = (allowedGroupIds ?? new int[0]).Distinct().ToList();
            if (allowed.Count == 0) return new List<ProjectGroupNode>();

            var all = LoadAllInternal();
            if (all.Count == 0) return new List<ProjectGroupNode>();

            // Filter: allowed groups + cac ancestor cua chung (de hien nhom)
            var byId = all.ToDictionary(g => g.IDGroup);
            var allowedSet = new HashSet<int>(allowed);
            var visible = new HashSet<int>(allowedSet);
            foreach (var gid in allowed)
            {
                ProjectGroupNode cur;
                if (!byId.TryGetValue(gid, out cur)) continue;
                while (cur != null && cur.ParentIDGroup.HasValue)
                {
                    visible.Add(cur.ParentIDGroup.Value);
                    if (!byId.TryGetValue(cur.ParentIDGroup.Value, out cur)) break;
                }
            }

            return BuildTreeFromFlat(all.Where(g => visible.Contains(g.IDGroup)).ToList(), allowedSet);
        }

        /// <summary>
        /// Admin: tat ca groups, khong filter quyen. Build N-cap tree.
        /// </summary>
        public static List<ProjectGroupNode> GetAllTree()
        {
            var all = LoadAllInternal();
            return BuildTreeFromFlat(all, null);
        }

        /// <summary>
        /// Admin: tat ca groups dang flat list (cho dropdown parent picker).
        /// </summary>
        public static List<ProjectGroupNode> GetAllFlat()
        {
            return LoadAllInternal();
        }

        public static ProjectGroupNode GetById(int id)
        {
            var all = LoadAllInternal();
            return all.FirstOrDefault(g => g.IDGroup == id);
        }

        private static List<ProjectGroupNode> LoadAllInternal()
        {
            var all = new List<ProjectGroupNode>();
            try
            {
                using (var conn = new SqlConnection(GetSqlConnString()))
                {
                    conn.Open();
                    bool hasParentCol = HasParentColumn(conn);
                    bool hasSortCol = HasColumn(conn, "SortOrder");

                    string sql;
                    if (hasParentCol && hasSortCol)
                        sql = "SELECT IDGroup, GroupName, ParentIDGroup, SortOrder FROM dbo.ProjectsGroup ORDER BY ParentIDGroup, SortOrder, GroupName";
                    else if (hasParentCol)
                        sql = "SELECT IDGroup, GroupName, ParentIDGroup, 0 AS SortOrder FROM dbo.ProjectsGroup ORDER BY ParentIDGroup, GroupName";
                    else
                        sql = "SELECT IDGroup, GroupName, NULL AS ParentIDGroup, 0 AS SortOrder FROM dbo.ProjectsGroup ORDER BY GroupName";

                    using (var cmd = new SqlCommand(sql, conn))
                    using (var rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            all.Add(new ProjectGroupNode
                            {
                                IDGroup = rd.GetInt32(0),
                                GroupName = rd.IsDBNull(1) ? "" : rd.GetString(1),
                                ParentIDGroup = rd.IsDBNull(2) ? (int?)null : rd.GetInt32(2),
                                SortOrder = rd.IsDBNull(3) ? 0 : rd.GetInt32(3)
                            });
                        }
                    }

                    // Project count per group - phan biet so projects truc tiep
                    try
                    {
                        using (var cmd = new SqlCommand(
                            "SELECT IDGroup, COUNT(*) FROM dbo.Projects WHERE IDGroup IS NOT NULL GROUP BY IDGroup", conn))
                        using (var rd = cmd.ExecuteReader())
                        {
                            var byId = all.ToDictionary(g => g.IDGroup);
                            while (rd.Read())
                            {
                                if (rd.IsDBNull(0)) continue;
                                var gid = rd.GetInt32(0);
                                ProjectGroupNode g;
                                if (byId.TryGetValue(gid, out g)) g.ProjectCount = rd.GetInt32(1);
                            }
                        }
                    }
                    catch { /* table name khac/khong ton tai - skip */ }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[ProjectsGroupHierarchy.LoadAll] " + ex.Message);
            }
            return all;
        }

        /// <summary>
        /// Build N-level tree tu flat list. Neu allowedSet != null, chi hien children allowed.
        /// </summary>
        private static List<ProjectGroupNode> BuildTreeFromFlat(List<ProjectGroupNode> flat, HashSet<int> allowedSet)
        {
            var byId = flat.ToDictionary(g => g.IDGroup);
            var roots = new List<ProjectGroupNode>();
            foreach (var g in flat)
            {
                g.Children = new List<ProjectGroupNode>();
            }
            foreach (var g in flat)
            {
                if (g.ParentIDGroup.HasValue && byId.ContainsKey(g.ParentIDGroup.Value))
                {
                    byId[g.ParentIDGroup.Value].Children.Add(g);
                }
                else
                {
                    roots.Add(g);
                }
            }
            // Sort children: SortOrder ASC, fallback GroupName ASC (de stable khi SortOrder bang nhau)
            foreach (var g in flat)
            {
                IEnumerable<ProjectGroupNode> src = g.Children;
                if (allowedSet != null) src = src.Where(c => allowedSet.Contains(c.IDGroup));
                g.Children = src.OrderBy(c => c.SortOrder).ThenBy(c => c.GroupName).ToList();
            }
            roots = roots.OrderBy(r => r.SortOrder).ThenBy(r => r.GroupName).ToList();
            AssignDepth(roots, 0);
            return roots;
        }

        private static void AssignDepth(List<ProjectGroupNode> nodes, int depth)
        {
            foreach (var n in nodes)
            {
                n.Depth = depth;
                AssignDepth(n.Children, depth + 1);
            }
        }

        /// <summary>
        /// Flatten tree theo thu tu pre-order (cho dropdown indent).
        /// </summary>
        public static List<ProjectGroupNode> FlattenPreOrder(List<ProjectGroupNode> roots)
        {
            var result = new List<ProjectGroupNode>();
            foreach (var r in roots) FlattenRec(r, result);
            return result;
        }

        private static void FlattenRec(ProjectGroupNode n, List<ProjectGroupNode> outList)
        {
            outList.Add(n);
            foreach (var c in n.Children) FlattenRec(c, outList);
        }

        /// <summary>
        /// Tat ca descendant IDs cua mot group (de check khi chon parent, tranh chu trinh).
        /// </summary>
        public static HashSet<int> GetDescendantIds(int rootId)
        {
            var all = LoadAllInternal();
            var byParent = all.GroupBy(g => g.ParentIDGroup ?? 0).ToDictionary(g => g.Key, g => g.ToList());
            var result = new HashSet<int>();
            var stack = new Stack<int>();
            stack.Push(rootId);
            while (stack.Count > 0)
            {
                var cur = stack.Pop();
                List<ProjectGroupNode> kids;
                if (!byParent.TryGetValue(cur, out kids)) continue;
                foreach (var k in kids)
                {
                    if (result.Add(k.IDGroup)) stack.Push(k.IDGroup);
                }
            }
            return result;
        }

        /// <summary>
        /// Backward compat - lay group + cac children direct (1-cap).
        /// </summary>
        public static List<int> GetGroupAndDescendantIds(int parentId)
        {
            var result = new List<int> { parentId };
            try
            {
                using (var conn = new SqlConnection(GetSqlConnString()))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(
                        "SELECT IDGroup FROM dbo.ProjectsGroup WHERE ParentIDGroup = @pid", conn))
                    {
                        cmd.Parameters.AddWithValue("@pid", parentId);
                        using (var rd = cmd.ExecuteReader())
                            while (rd.Read()) result.Add(rd.GetInt32(0));
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[ProjectsGroupHierarchy.GetGroupAndDescendantIds] " + ex.Message);
            }
            return result;
        }

        /// <summary>
        /// Build SelectList cho dropdown gan project voi group. Hien thi indent theo Depth
        /// (4 space cho moi cap) de user thay duoc cau truc hierarchy khi chon.
        /// </summary>
        public static System.Web.Mvc.SelectList BuildSelectList(int? selectedId)
        {
            var tree = GetAllTree();
            var flat = FlattenPreOrder(tree);
            var items = flat.Select(n => new
            {
                IDGroup = n.IDGroup,
                GroupName = string.Concat(System.Linq.Enumerable.Repeat("    ", n.Depth)) + n.GroupName
            }).ToList();
            return new System.Web.Mvc.SelectList(items, "IDGroup", "GroupName", selectedId);
        }

        // ============================================================
        // Admin CRUD - raw SQL vi SP cu khong nhan ParentIDGroup
        // ============================================================

        public static int Insert(string groupName, int? parentIdGroup)
        {
            using (var conn = new SqlConnection(GetSqlConnString()))
            {
                conn.Open();
                bool hasParentCol = HasParentColumn(conn);
                bool hasSortCol = HasColumn(conn, "SortOrder");

                // Compute next SortOrder = max sibling SortOrder + 1 (de group moi nam cuoi cung)
                int nextSort = 1;
                if (hasSortCol)
                {
                    using (var maxCmd = new SqlCommand(
                        hasParentCol
                            ? "SELECT ISNULL(MAX(SortOrder), 0) + 1 FROM dbo.ProjectsGroup WHERE (ParentIDGroup = @pid OR (ParentIDGroup IS NULL AND @pid IS NULL))"
                            : "SELECT ISNULL(MAX(SortOrder), 0) + 1 FROM dbo.ProjectsGroup", conn))
                    {
                        if (hasParentCol)
                            maxCmd.Parameters.AddWithValue("@pid", (object)parentIdGroup ?? DBNull.Value);
                        nextSort = Convert.ToInt32(maxCmd.ExecuteScalar() ?? 1);
                    }
                }

                string sql;
                if (hasParentCol && hasSortCol)
                    sql = "INSERT INTO dbo.ProjectsGroup (GroupName, ParentIDGroup, SortOrder) OUTPUT INSERTED.IDGroup VALUES (@name, @pid, @sort)";
                else if (hasParentCol)
                    sql = "INSERT INTO dbo.ProjectsGroup (GroupName, ParentIDGroup) OUTPUT INSERTED.IDGroup VALUES (@name, @pid)";
                else
                    sql = "INSERT INTO dbo.ProjectsGroup (GroupName) OUTPUT INSERTED.IDGroup VALUES (@name)";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@name", groupName ?? "");
                    if (hasParentCol)
                        cmd.Parameters.AddWithValue("@pid", (object)parentIdGroup ?? DBNull.Value);
                    if (hasParentCol && hasSortCol)
                        cmd.Parameters.AddWithValue("@sort", nextSort);
                    var res = cmd.ExecuteScalar();
                    return res != null ? Convert.ToInt32(res) : 0;
                }
            }
        }

        /// <summary>
        /// Move group LEN (giam SortOrder so voi sibling truoc) - swap SortOrder voi neighbor.
        /// Sibling = cung ParentIDGroup. Tra ve true neu di chuyen duoc, false neu da o dau.
        /// </summary>
        public static bool MoveUp(int idGroup)
        {
            return SwapWithNeighbor(idGroup, -1);
        }

        /// <summary>
        /// Move group XUONG (tang SortOrder so voi sibling sau) - swap voi neighbor.
        /// </summary>
        public static bool MoveDown(int idGroup)
        {
            return SwapWithNeighbor(idGroup, +1);
        }

        private static bool SwapWithNeighbor(int idGroup, int direction)
        {
            using (var conn = new SqlConnection(GetSqlConnString()))
            {
                conn.Open();
                if (!HasColumn(conn, "SortOrder"))
                    throw new InvalidOperationException("Cot SortOrder chua co tren DB nay. Hay chay migration v360-projectsgroup-sortorder.sql truoc.");

                // 1. Lay current SortOrder + ParentIDGroup cua group
                int curSort; int? curParent;
                bool hasParentCol = HasParentColumn(conn);
                using (var cmd = new SqlCommand(
                    hasParentCol
                        ? "SELECT SortOrder, ParentIDGroup FROM dbo.ProjectsGroup WHERE IDGroup=@id"
                        : "SELECT SortOrder, NULL FROM dbo.ProjectsGroup WHERE IDGroup=@id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", idGroup);
                    using (var rd = cmd.ExecuteReader())
                    {
                        if (!rd.Read()) return false;
                        curSort = rd.GetInt32(0);
                        curParent = rd.IsDBNull(1) ? (int?)null : rd.GetInt32(1);
                    }
                }

                // 2. Tim sibling neighbor theo direction
                string siblingFilter = hasParentCol
                    ? "(ParentIDGroup = @pid OR (ParentIDGroup IS NULL AND @pid IS NULL))"
                    : "1=1";
                string neighborSql = direction < 0
                    ? "SELECT TOP 1 IDGroup, SortOrder FROM dbo.ProjectsGroup WHERE " + siblingFilter + " AND IDGroup<>@id AND SortOrder<@s ORDER BY SortOrder DESC, GroupName DESC"
                    : "SELECT TOP 1 IDGroup, SortOrder FROM dbo.ProjectsGroup WHERE " + siblingFilter + " AND IDGroup<>@id AND SortOrder>@s ORDER BY SortOrder ASC, GroupName ASC";

                int neighborId; int neighborSort;
                using (var cmd = new SqlCommand(neighborSql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", idGroup);
                    cmd.Parameters.AddWithValue("@s", curSort);
                    if (hasParentCol)
                        cmd.Parameters.AddWithValue("@pid", (object)curParent ?? DBNull.Value);
                    using (var rd = cmd.ExecuteReader())
                    {
                        if (!rd.Read()) return false;  // da o dau/cuoi
                        neighborId = rd.GetInt32(0);
                        neighborSort = rd.GetInt32(1);
                    }
                }

                // 3. Edge case: neu neighbor co SortOrder bang current (init data co the tie),
                // bump neighbor lui hoac toi 1 don vi de tao gap, roi swap.
                if (neighborSort == curSort)
                {
                    neighborSort = curSort + (direction < 0 ? -1 : 1);
                }

                // 4. Swap SortOrder
                using (var tx = conn.BeginTransaction())
                {
                    using (var cmd = new SqlCommand("UPDATE dbo.ProjectsGroup SET SortOrder=@s WHERE IDGroup=@id", conn, tx))
                    {
                        cmd.Parameters.AddWithValue("@s", neighborSort);
                        cmd.Parameters.AddWithValue("@id", idGroup);
                        cmd.ExecuteNonQuery();
                    }
                    using (var cmd = new SqlCommand("UPDATE dbo.ProjectsGroup SET SortOrder=@s WHERE IDGroup=@id", conn, tx))
                    {
                        cmd.Parameters.AddWithValue("@s", curSort);
                        cmd.Parameters.AddWithValue("@id", neighborId);
                        cmd.ExecuteNonQuery();
                    }
                    tx.Commit();
                }
                return true;
            }
        }

        public static void Update(int idGroup, string groupName, int? parentIdGroup)
        {
            // Chong chu trinh: neu chon parent la chinh no hoac descendant -> reject
            if (parentIdGroup.HasValue)
            {
                if (parentIdGroup.Value == idGroup)
                    throw new InvalidOperationException("Không thể chọn chính nó làm nhóm cha");
                var desc = GetDescendantIds(idGroup);
                if (desc.Contains(parentIdGroup.Value))
                    throw new InvalidOperationException("Không thể chọn nhóm con làm nhóm cha (gây vòng lặp)");
            }
            using (var conn = new SqlConnection(GetSqlConnString()))
            {
                conn.Open();
                bool hasParentCol = HasParentColumn(conn);
                var sql = hasParentCol
                    ? "UPDATE dbo.ProjectsGroup SET GroupName=@name, ParentIDGroup=@pid WHERE IDGroup=@id"
                    : "UPDATE dbo.ProjectsGroup SET GroupName=@name WHERE IDGroup=@id";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", idGroup);
                    cmd.Parameters.AddWithValue("@name", groupName ?? "");
                    if (hasParentCol)
                        cmd.Parameters.AddWithValue("@pid", (object)parentIdGroup ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Xoa group. Antaract: tu choi neu group co children HOAC co projects truc tiep.
        /// Throw InvalidOperationException neu khong an toan.
        /// </summary>
        public static void Delete(int idGroup)
        {
            using (var conn = new SqlConnection(GetSqlConnString()))
            {
                conn.Open();
                bool hasParentCol = HasParentColumn(conn);
                // Check children
                if (hasParentCol)
                {
                    using (var cmd = new SqlCommand(
                        "SELECT COUNT(*) FROM dbo.ProjectsGroup WHERE ParentIDGroup=@id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", idGroup);
                        var n = Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
                        if (n > 0)
                            throw new InvalidOperationException("Nhóm còn chứa " + n + " nhóm con. Vui lòng xóa/di chuyển các nhóm con trước.");
                    }
                }
                // Check projects (table name tu EDMX: Projects)
                try
                {
                    using (var cmd = new SqlCommand(
                        "SELECT COUNT(*) FROM dbo.Projects WHERE IDGroup=@id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", idGroup);
                        var n = Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
                        if (n > 0)
                            throw new InvalidOperationException("Nhóm còn chứa " + n + " dự án. Vui lòng xóa/di chuyển các dự án trước.");
                    }
                }
                catch (InvalidOperationException) { throw; }
                catch { /* bang khong ton tai -> skip */ }

                using (var cmd = new SqlCommand("DELETE FROM dbo.ProjectsGroup WHERE IDGroup=@id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", idGroup);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
