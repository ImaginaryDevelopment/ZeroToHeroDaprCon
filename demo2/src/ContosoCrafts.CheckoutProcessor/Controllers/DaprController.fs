namespace ContosoCrafts.CheckoutProcessor.Controllers

open System.Threading.Tasks;
open Microsoft.AspNetCore.Mvc;
open Microsoft.Extensions.Logging;

open CloudNative.CloudEvents;
open CloudNative.CloudEvents.AspNetCore;
open CloudNative.CloudEvents.SystemTextJson;

module LocalControllerSettings =
    let useFSharpList = true
[<ApiController>]
[<Route("[controller]")>]
// constructor arguments are available as fields automatically
type DaprController(logger: ILogger<DaprController>) =
    inherit ControllerBase()

    [<HttpGet("subscribe")>]
    member this.Subscribe() : ActionResult =
        // assuming this was an anonymous type array
        let payload = [
            {| pubsubname="rabbitmqbus"; topic= "checkout"; route = "checkout" |}
        ]
        if LocalControllerSettings.useFSharpList then
            upcast this.Ok(payload)
        else
            // ResizeArray is an F# alias for C#'s normal list
            // System.Collections.Generic.List<T>
            upcast this.Ok(ResizeArray(payload))

    [<HttpPost("/checkout")>]
    // public async Task<ActionResult> CheckoutOrder()
    // implicit casting is frowned upon in F#, but in some places it works
    member this.CheckoutOrder(): Task<ActionResult> =
        task {
            let jsonFormatter = JsonEventFormatter();
            let! cloudEvent = this.Request.ToCloudEventAsync(jsonFormatter);
            logger.LogDebug($"Cloud event {cloudEvent.Id} {cloudEvent.Type} {cloudEvent.DataContentType}");
            logger.LogInformation("Order received...");
            return this.Ok() :> ActionResult;
        }