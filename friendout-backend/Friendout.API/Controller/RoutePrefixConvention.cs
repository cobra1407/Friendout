using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace friendout_backend.Controller;

/// <summary>
/// Route prefix convention
/// </summary>
public class RoutePrefixConvention : IApplicationModelConvention
{
    private readonly AttributeRouteModel _routePrefix;

    /// <summary>
    /// Initializes a new instance of the <see cref="RoutePrefixConvention"/> class.
    /// </summary>
    /// <param name="prefix"></param>
    public RoutePrefixConvention(string prefix)
    {
        _routePrefix = new AttributeRouteModel(new Microsoft.AspNetCore.Mvc.RouteAttribute(prefix));
    }

    /// <summary>
    /// Apply the route prefix to all controllers 
    /// </summary>
    /// <param name="application"></param>
    public void Apply(ApplicationModel application)
    {
        foreach (var controller in application.Controllers)
        {
            foreach (var selector in controller.Selectors)
            {
                if (selector.AttributeRouteModel != null)
                {
                    // Combine existing route with prefix
                    selector.AttributeRouteModel = 
                        AttributeRouteModel.CombineAttributeRouteModel(_routePrefix, selector.AttributeRouteModel);
                }
                else
                {
                    // No route specified: apply prefix as the route
                    selector.AttributeRouteModel = _routePrefix;
                }
            }
        }
    }
}