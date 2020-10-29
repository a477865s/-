using System.Web;
using System.Web.Optimization;

namespace EIP_System
{
    public class BundleConfig
    {
        // 如需統合的詳細資訊，請瀏覽 https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // 使用開發版本的 Modernizr 進行開發並學習。然後，當您
            // 準備好可進行生產時，請使用 https://modernizr.com 的建置工具，只挑選您需要的測試。
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js"));

            //css
            bundles.Add(new StyleBundle("~/Content/css").Include(
                        "~/Content/bootstrap.css",
                        "~/Content/site.css",
                        //gentelella - master
                        "~/Plugin/gentelella-master/custom.css",
                        "~/Plugin/gentelella-master/vendors/font-awesome/css/font-awesome.min.css",
                        //DataTables
                        "~/Plugin/DataTables/datatables.min.css",

                        //Fullcalendar
                        "~/Plugin/Fullcalendar/main.min.css",
                        //Chart
                        "~/Content/Chart.min.css",
                        //
                        "~/Content/Index.css"
                      ));


            //customjs
            bundles.Add(new ScriptBundle("~/bundles/customjs").Include(
                        //gentelella - master
                        "~/Plugin/gentelella-master/custom.js",
                        //DataTables
                        "~/Plugin/DataTables/datatables.min.js",
                        //Chartjs
                        "~/Scripts/Chart.min.js",
                        //moment
                        "~/Scripts/moment.min.js",
                        "~/Scripts/moment-with-locales.min.js",
                        //Fullcalendar
                        "~/Plugin/Fullcalendar/main.js",
                        "~/Plugin/Fullcalendar/locales-all.min.js",
                        //SignalR
                        "~/Scripts/jquery.signalR-2.4.1.js",
                        "~/signalr/hubs",
                        //Notify
                        "~/Plugin/Notify/notify.js"
                        
                        ));
        }
    }
}
