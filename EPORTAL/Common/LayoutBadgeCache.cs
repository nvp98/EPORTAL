using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EPORTAL.ModelsTagSign;

namespace EPORTAL.Common
{
    /// <summary>
    /// 5 badge counts hien tren _Layout menu (TheNguoi/CapMoi/GiaHan/BoSung/CapLai).
    /// </summary>
    public class LayoutBadges
    {
        public int TheNguoi { get; set; }
        public int CapMoi   { get; set; }
        public int GiaHan   { get; set; }
        public int BoSung   { get; set; }
        public int CapLai   { get; set; }
    }

    /// <summary>
    /// Cache badge counts cho _Layout menu trong Session (60s expire).
    /// Truoc day moi page load chay ~5 outer query + 5*N inner -> total 20-50 query DB
    /// (tuy data). Cache 60s -> giam xuong 0 query phan badge cho hau het request.
    /// </summary>
    public static class LayoutBadgeCache
    {
        private const int CacheSeconds = 60;

        public static LayoutBadges Get(int nhanVienId, HttpSessionStateBase session)
        {
            var key = "v360_layout_badges_" + nhanVienId;
            if (session != null)
            {
                var entry = session[key] as Tuple<DateTime, LayoutBadges>;
                if (entry != null && (DateTime.UtcNow - entry.Item1).TotalSeconds < CacheSeconds)
                    return entry.Item2;
            }
            var fresh = Compute(nhanVienId);
            if (session != null) session[key] = Tuple.Create(DateTime.UtcNow, fresh);
            return fresh;
        }

        private static LayoutBadges Compute(int idnv)
        {
            var badges = new LayoutBadges();
            try
            {
                using (var db = new EPORTAL_REGISTEREntities())
                {
                    // === 3 batch queries thay vi N+M queries per group ===
                    // Project ve int (khong nullable) cho cac field can lam key.
                    var registers = db.RegisterPeoples
                        .Where(r => r.TinhTrang_ID == 1)
                        .Select(r => new { Dktn = r.ID_DKTN, TrinhKy = r.TrinhKy_ID })
                        .ToList();
                    if (registers.Count == 0) return badges;

                    var userFlows = db.SignOff_Flow
                        .Where(f => f.TinhTrangID == 0 && f.NhanVienID == idnv && f.DKTN_ID != null)
                        .Select(f => new { Dktn = f.DKTN_ID.Value, Cd = f.CapDuyet ?? 0 })
                        .ToList();
                    if (userFlows.Count == 0) return badges;

                    var relevantDktnIds = registers.Select(r => (int?)r.Dktn).Distinct().ToList();
                    var allFlows = db.SignOff_Flow
                        .Where(f => relevantDktnIds.Contains(f.DKTN_ID))
                        .Select(f => new {
                            Dktn = f.DKTN_ID ?? 0,
                            Cd   = f.CapDuyet ?? 0,
                            Tt   = f.TinhTrangID ?? 0,
                            TkTn = f.ID_TK_TN
                        })
                        .ToList();

                    var flowsByDktn = allFlows.GroupBy(f => f.Dktn).ToDictionary(g => g.Key, g => g.ToList());
                    var userFlowsByDktn = userFlows.GroupBy(f => f.Dktn).ToDictionary(g => g.Key, g => g.ToList());

                    // === In-memory iterate, replicate logic goc cua _Layout ===
                    foreach (var reg in registers)
                    {
                        int dktn = reg.Dktn;
                        if (!userFlowsByDktn.ContainsKey(dktn)) continue;

                        var flowsForDktn = userFlowsByDktn[dktn];
                        int flowsCount = flowsForDktn.Count;
                        if (flowsCount == 0) continue;
                        var allForDktn = flowsByDktn.ContainsKey(dktn) ? flowsByDktn[dktn] : null;

                        bool isTheNguoi = !reg.TrinhKy.HasValue || reg.TrinhKy.Value == 0;

                        foreach (var item in flowsForDktn)
                        {
                            int cd = item.Cd;
                            bool eligible;
                            if (cd == 1)
                            {
                                eligible = true;
                            }
                            else
                            {
                                int duyetCheck = cd - 1;
                                if (allForDktn == null) { eligible = false; }
                                else if (isTheNguoi)
                                {
                                    eligible = !allForDktn.Any(x => x.Cd <= duyetCheck && x.Tt != 1);
                                }
                                else
                                {
                                    eligible = !allForDktn.Any(x => x.TkTn == duyetCheck && x.Tt != 1);
                                }
                            }
                            if (!eligible) continue;

                            if (isTheNguoi) badges.TheNguoi += flowsCount;
                            else
                            {
                                switch (reg.TrinhKy.Value)
                                {
                                    case 1: badges.CapMoi += flowsCount; break;
                                    case 2: badges.GiaHan += flowsCount; break;
                                    case 3: badges.BoSung += flowsCount; break;
                                    case 4: badges.CapLai += flowsCount; break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[LayoutBadgeCache] Compute err: " + ex.Message);
            }
            return badges;
        }
    }
}
