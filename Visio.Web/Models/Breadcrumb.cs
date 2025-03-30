namespace Visio.Web.Models
{
    public class Breadcrumb
    {
        public string Label { get; }
        public string Action { get; }
        public string Controller { get; }
        public object RouteId { get; }

        public Breadcrumb(string label, string action, string controller, object routeId = null)
        {
            Label = label;
            Action = action;
            Controller = controller;
            RouteId = routeId;
        }
    }
}
