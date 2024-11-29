using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace EPORTAL.Models
{
    public class MyAuthentication
    {
        PhanQuyenHTEntities db = new PhanQuyenHTEntities();
        public static void ClearAuthentication()
        {
            FormsAuthentication.SignOut();
            HttpContext.Current.Session.Abandon();
        }
        public static int ID
        {
            get
            {
                try
                {
                    object obj = HttpContext.Current.User.Identity.Name.Split(';')[0];
                    return (obj == null) ? 0 : Convert.ToInt32(obj);
                }
                catch
                {
                    return 0;
                }
            }
        }
        public static string Username
        {
            get
            {
                try
                {
                    object obj = HttpContext.Current.User.Identity.Name.Split(';')[1];
                    return (obj == null) ? String.Empty : (string)obj;
                }
                catch
                {
                    return null;
                }
            }
        }
        public static string TenNV
        {
            get
            {
                try
                {

                    object obj = HttpContext.Current.User.Identity.Name.Split(';')[2];
                    return (obj == null) ? String.Empty : (string)obj;
                }
                catch
                {
                    return null;
                }
            }
        }
        public static int IDPhongban
        {
            get
            {
                try
                {
                    object obj = HttpContext.Current.User.Identity.Name.Split(';')[3];
                    return (obj == null) ? 0 : Convert.ToInt32(obj);
                }
                catch
                {
                    return 0;
                }
            }
        }
        public static int IDQuyen
        {
            get
            {
                try
                {
                    object obj = HttpContext.Current.User.Identity.Name.Split(';')[4];
                    return (obj == null) ? 0 : Convert.ToInt32(obj);
                }
                catch
                {
                    return 0;
                }
            }
        }
        public static int IDQuyenHT
        {
            get
            {
                try
                {
                    object obj = HttpContext.Current.User.Identity.Name.Split(';')[5];
                    return (obj == null) ? 0 : Convert.ToInt32(obj);
                }
                catch
                {
                    return 0;
                }
            }
        }
        public static string GroupQuyen
        {
            get
            {
                try
                {
                    object obj = HttpContext.Current.User.Identity.Name.Split(';')[6];
                    return (obj == null) ? String.Empty : (string)obj;
                }
                catch
                {
                    return null;
                }
            }
        }
        public static string NT
        {
            get
            {
                try
                {
                    object obj = HttpContext.Current.User.Identity.Name.Split(';')[7];
                    return (obj == null) ? String.Empty : (string)obj;
                }
                catch
                {
                    return null;
                }
            }
        }

        public List<String> GetPermisionCN(int? IDQuyenHT, string ControllerName)
        {
            //var IdControll = db.ListControllers.Where(x => x.Controller == ControllerName).Select(x => x.ID).FirstOrDefault();
            //var lsQuyen = (from a in db.A_QuyenCT.Where(x => x.IDQuyenHT == IDQuyenHT && x.isActive == 1)
            //               join b in db.A_QuyenCN on a.IDQuyenCN equals b.IDQuyenCN
            //               join c in db.A_ListController.Where(x => x.Controller == ControllerName && x.isActive == 1) on a.IDController equals c.IDController
            //               select b.MaQuyenCN).ToList();
            var lsQuyen = db.A_CheckListQuyenActive(IDQuyenHT, ControllerName).ToList();
            //if (!lsQuyen.Contains("VIEW")) lsQuyen = new List<string>();
            var GroupQuyen = MyAuthentication.GroupQuyen;
            if (GroupQuyen != null && !String.IsNullOrEmpty(GroupQuyen))
            {
                var grQuyen = GroupQuyen.Split(',');
                foreach (var item in grQuyen)
                {
                    int idq = Convert.ToInt32(item);
                    //List<String> lsQuyen2 = (from a in db.A_QuyenCT.Where(x => x.IDQuyenHT == idq && x.isActive == 1)
                    //                         join b in db.A_QuyenCN on a.IDQuyenCN equals b.IDQuyenCN
                    //                         join c in db.A_ListController.Where(x => x.Controller == ControllerName && x.isActive == 1) on a.IDController equals c.IDController
                    //                         select b.MaQuyenCN).ToList();
                    var lsQuyen2 = db.A_CheckListQuyenActive(idq, ControllerName).ToList();
                    if (lsQuyen2.Count > 0)
                    {
                        foreach (var ls in lsQuyen2)
                        {
                            lsQuyen.Add(ls);
                        }
                    }
                }
            }
            return lsQuyen;
        }
    }
}