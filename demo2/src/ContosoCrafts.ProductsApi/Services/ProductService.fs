namespace ContosoCrafts.ProductsApi.Services

open System
open System.Collections.Generic
open System.Threading.Tasks

open MongoDB.Bson
open MongoDB.Driver

open ContosoCrafts.ProductsApi.Models

// work around default/optional args not directly in abstract classes in F#
// https://www.reddit.com/r/fsharp/comments/e1kdf0/how_does_one_implement_default_values_for/
type PagingInfo(page:int option, limit:int option) =
    new(page: int Nullable, limit: int Nullable) =
        PagingInfo(Option.ofNullable page, Option.ofNullable limit)
    member this.Page =
        defaultArg page 1
    member this.Limit =
        defaultArg limit 20

type IProductService =
    // Task<IEnumerable<Product>> GetProducts(int page = 1, int limit = 20)
    abstract member GetProducts: PagingInfo -> Task<IEnumerable<Product>>
    // Task AddRating(string productId, int rating)
    abstract member AddRating: productId:string * rating:int -> Task
    // Task<Product> GetSingle(string id)
    abstract member GetSingle: id:string -> Task<Product>

type ProductService (mongo:IMongoClient) =
    let DATABASE_NAME = "contosocrafts"
    let COLLECTION_NAME = "products"
    let database: IMongoDatabase = mongo.GetDatabase(DATABASE_NAME)

    // public async Task AddRating(string productId, int rating)
    member this.AddRating(productId:string, rating:int ) : Task =
        task{
            let collection = database.GetCollection<Product>(COLLECTION_NAME)
            let filter = Builders<Product>.Filter.Eq((fun x -> x.ProductId), productId)
            let update = Builders<Product>.Update.Push((fun x -> x.Ratings |> Seq.ofArray), rating)
            let! result = Async.AwaitTask(collection.UpdateOneAsync(filter, update))
            ()
        }

    // public async Task<IEnumerable<Product>> GetProducts(int page = 1, int limit = 20)
    member this.GetProducts(pagingInfo: PagingInfo) : Task<IEnumerable<_>> =
        task{
            let collection : IMongoCollection<Product> = database.GetCollection<Product>(COLLECTION_NAME)
            let fd: FilterDefinition<Product> = FilterDefinition.op_Implicit(new BsonDocument())
            let query =
                collection.Find(fd)
                    .Skip(Convert.ToInt32((pagingInfo.Page - 1) * pagingInfo.Limit))
                    .Limit(Convert.ToInt32(pagingInfo.Limit))

            let! results = query.ToListAsync()
            return results :> IEnumerable<_>
        }

    // public async Task<Product> GetSingle(string id)
    member this.GetSingle(id:string) =
        task{
            let collection = database.GetCollection<Product>(COLLECTION_NAME)
            let filter = Builders<Product>.Filter.Eq((fun x -> x.ProductId), id)
            let! cursor = collection.FindAsync(filter)
            return cursor.SingleOrDefault()
        }

    // F# pain point, no implicit interface implementation
    interface IProductService with
        member this.GetProducts(x) = this.GetProducts(x)
        member this.AddRating(productId,rating) = this.AddRating(productId, rating)
        member this.GetSingle(id) = this.GetSingle(id)

