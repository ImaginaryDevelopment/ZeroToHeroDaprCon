namespace ContosoCrafts.CheckoutProcessor

// left semicolons to be more C#-ish
open Microsoft.AspNetCore.Builder;
open Microsoft.Extensions.Configuration;
open Microsoft.Extensions.DependencyInjection;

type Startup(configuration: IConfiguration) =

    member _.Configuration = configuration;

    member _.ConfigureServices(services: IServiceCollection) : unit =
        services.AddControllers()
        |> ignore

    member _.Configure(app: IApplicationBuilder) : unit =
        // warning here for not using a proper ignore
        app.UseRouting()
        |> ignore
        app.UseEndpoints(fun endpoints ->
            // no ignore here would be an error for a lambda function
            endpoints.MapControllers()
            |> ignore<ControllerActionEndpointConventionBuilder>
        )
        |> ignore