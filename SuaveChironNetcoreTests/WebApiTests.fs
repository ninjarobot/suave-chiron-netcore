namespace SuaveChironNetcoreTests

open System.Net
open System.Net.Http
open System.Threading
open Xunit
open Suave
open Suave.Filters
open Suave.Operators
open SuaveChironNetcore.WebApi
open SuaveChironNetcore.HNProvider

module WebApiTests =

    type Tests() =

        let rng = System.Random ()
        let testPort = rng.Next(32768, 65535) |> uint16

        let testInServer app test = 
            let start, shutdown = startWebServerAsync { defaultConfig with bindings = [HttpBinding.create HTTP IPAddress.Loopback testPort] } app
            use cts = new CancellationTokenSource()
            Async.Start (shutdown, cts.Token)
            start |> Async.RunSynchronously |> ignore
            try
                test ()
            finally
                cts.Cancel ()

        [<Fact>]
        let ``get a news item`` () =
            let mockGetNewsItem _ =
                async {
                    let metadata =
                        {
                            Deleted = None
                            Dead = None
                            By = "justin"
                            Id = 100
                            Kids = [|101 ; 102|] |> Some
                            Time = System.DateTime(2017, 10, 25, 16, 04, 35)
                        }
                    return Job(metadata, "SomeJob")
                }
            let handler = newsItemHandler mockGetNewsItem
            let app = choose [ GET >=> pathScan "/news/%i" handler ]
            testInServer app (fun () ->
                use http = new HttpClient ()
                let testUri = sprintf "http://localhost:%d/news/1234" testPort
                use res = http.GetAsync (testUri) |> Async.AwaitTask |> Async.RunSynchronously
                Assert.Equal (HttpStatusCode.OK, res.StatusCode)
                let body = res.Content.ReadAsStringAsync () |> Async.AwaitTask |> Async.RunSynchronously
                Assert.Equal ("""{"by":"justin","id":100,"kids":[101,102],"time":1508961875,"title":"SomeJob","type":"job"}""", body)
            )