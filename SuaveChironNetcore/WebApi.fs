namespace SuaveChironNetcore

open Suave
open Suave.Filters
open Suave.Operators
open Suave.Successful
open Suave.RequestErrors
open Suave.Writers
open HNProvider

module WebApi = 

    let topStoriesHandler =
        fun (ctx:HttpContext) -> async {
            let! news = topNews ()
            return! OK (sprintf "%A" news) ctx 
        }

    let newsItemHandler (id:int) =
        fun (ctx:HttpContext) -> async {
            let! item = newsItem id
            let title = sprintf "%A" item
            return! OK title ctx
        }

    let startServer () =
        let app = 
            choose
                [ GET >=> choose 
                    [
                        path "/topstories" >=> topStoriesHandler >=> setMimeType "application/json; charset=utf-8"
                        pathScan "/story/%i" newsItemHandler >=> setMimeType "application/json; charset=utf-8"
                        path "/favicon.ico" >=> ok [||] >=> setMimeType "image/x-icon" // QUIT IT FAVICON!!!
                    ]
                ]
        startWebServer { defaultConfig with hideHeader=true } app
