﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace EPORTAL.ModelsDanhBaDoiTac
{
    public class MyAuthentication2
    {
        public static void ClearAuthentication()
        {
            FormsAuthentication.SignOut();
            HttpContext.Current.Session.Abandon();
        }
        public static int IDDoiTac
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
        public static string ShortName
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
        public static int IDQuyen
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

    }
}