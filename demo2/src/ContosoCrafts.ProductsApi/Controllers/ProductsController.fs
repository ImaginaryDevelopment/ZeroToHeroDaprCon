namespace ContosoCrafts.ProductsApi.Controllers

open System
open System.Threading.Tasks
open Microsoft.AspNetCore.Mvc

open ContosoCrafts.ProductsApi.Services


// assuming the framework will be ok with immutables here
// if not uncomment the attribute and try that
// [<CLIMutable>]
type RatingRequest = {ProductId: string; Rating: int}

[<ApiController>]
[<Route("[controller]")>]
type ProductsController(productService: IProductService) =
    inherit ControllerBase()

    [<HttpGet>]
    // public async Task<ActionResult> GetList(int page = 1, int limit = 20)
    member this.GetList(page : int Nullable, limit: int Nullable) : Task<IActionResult> =
        let pagingInfo = PagingInfo(page,limit)
        task{
            let! result = productService.GetProducts pagingInfo
            return this.Ok(result) :> IActionResult
        }

    [<HttpGet("{id}")>]
    // public async Task<ActionResult> GetSingle(string id)
    member this.GetSingle(id: string) =
        task{
            let! result = productService.GetSingle(id)
            return this.Ok(result) :> IActionResult
        }

    [<HttpPatch>]
    // public async Task<ActionResult> Patch(RatingRequest request)
    member this.Patch(request:RatingRequest): Task<IActionResult> =
        task{
            do! productService.AddRating(request.ProductId, request.Rating)
            return this.Ok() :> IActionResult
        }
