using Sitecore.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterToWeb.Feature.PageContent
{
    public class Constants
    {
        public struct Templates
        {
            public struct GlobalPageStructure
            {
                public static readonly ID ID = new ID("{509978E3-D69D-4422-A3C1-97EDE9714887}");
                public struct Fields
                {
                    public static readonly ID HeaderItem = new ID("{E92682B6-116A-4E05-8993-C584207CEBD4}");
                    public static readonly ID FooterItem = new ID("{F19387FF-E43C-4563-B7DD-9573237586ED}");
                }
            }

            public struct HasGlobalStructure
            {
                public static readonly ID ID = new ID("{C2BF62A7-D246-4FC1-AB87-9A062C5F8072}");
                public struct Fields
                {
                    public static readonly ID UseGlobalHeader = new ID("{7C004AE3-143A-497E-A1C6-B0E87524D6F6}");
                    public static readonly ID UseGlobalFooter = new ID("{D7166AA5-E099-461F-9925-683F0459C87F}");
                }
            }

            public struct HeaderPage
            {
                public static readonly ID ID = new ID("{E36AD52B-CDEE-41F9-9745-040FCD5D9272}");
            }  
            
            public struct FooterPage
            {
                public static readonly ID ID = new ID("{6EAD5675-F7A9-42E7-97E7-33DA178BD14C}");
            }
        }
    }
}