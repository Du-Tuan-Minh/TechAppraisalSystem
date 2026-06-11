using Microsoft.AspNetCore.Mvc.Filters;

namespace Application.Extensions
{
    public class NullEmptyGuidFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            foreach (var argument in context.ActionArguments.Values)
            {
                if (argument == null) continue;

                var properties = argument.GetType().GetProperties()
                    .Where(p => p.PropertyType == typeof(Guid?) && p.CanWrite);

                foreach (var prop in properties)
                {
                    var value = (Guid?)prop.GetValue(argument);
                    if (value == Guid.Empty)
                    {
                        prop.SetValue(argument, null);
                    }
                }
            }
        }

        public void OnActionExecuted(ActionExecutedContext context) { }
    }
}
