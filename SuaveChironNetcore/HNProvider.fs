namespace SuaveChironNetcore

open Chiron.Builder
open Chiron.Mapping
open Chiron.Operators
open Chiron.Parsing

/// Interaction with Hacker News API: https://github.com/HackerNews/API
module HNProvider =

    [<Literal>]
    let BaseUri = "https://hacker-news.firebaseio.com/v0"

    let private fromUnixTime time = 
        System.DateTimeOffset.FromUnixTimeSeconds(time).UtcDateTime

    let private toUnixTime time =
        System.DateTimeOffset(time).ToUnixTimeSeconds ()

    type Metadata = {
        Deleted : bool option
        Dead : bool option
        By : string // author
        Id : int
        Kids : int array option
        Time : System.DateTime // Creation date of the item, in Unix Time.
    }
    with
        static member ToJson (m:Metadata) =
            json {
                do! Json.write "id" m.Id
                do! Json.writeUnlessDefault "dead" None m.Dead
                do! Json.write "by" m.By
                do! Json.writeUnlessDefault "kids" None m.Kids
                do! Json.write "time" (m.Time |> toUnixTime)
                do! Json.writeUnlessDefault "deleted" None m.Deleted
            }
        
    type CommentCount = int
    type ParentID = int
    type Score = int
    type Votes = int // score on a pollopt
    type Title = string
    type Url = string
    type PollOptions = int array
    type Text = string

    type HackerNewsItem = 
        | Job of Metadata * Title
        | Story of Metadata * Title * Url option * CommentCount * Score * Text option
        | Comment of Metadata * ParentID * Text
        | Poll of Metadata * Title * CommentCount * PollOptions * Text
        | PollOpt of Metadata * ParentID * Text * Votes

    with
        static member ToJson (h:HackerNewsItem) =
            match h with
            | Job (metadata, title) ->
                json {
                    do! Metadata.ToJson (metadata)
                    do! Json.write "type" "job"
                    do! Json.write "title" title
                }
            | Story(_, _, _, _, _, _) -> failwith "Not Implemented"
            | Comment(_, _, _) -> failwith "Not Implemented"
            | Poll(_, _, _, _, _) -> failwith "Not Implemented"
            | PollOpt(_, _, _, _) -> failwith "Not Implemented"            

        static member FromJson (_:HackerNewsItem) =
                fun id deleted typ by time text dead parent kids url score title parts descendants ->
                    let metadata = {
                        By = by
                        Dead = dead
                        Deleted = deleted
                        Id = id
                        Kids = kids
                        Time = time |> fromUnixTime
                        }
                    match typ with 
                    | "job" -> 
                        match title with
                        | None -> failwith "Job missing title"
                        | Some t -> Job (metadata, t)
                    | "story" -> 
                        match title with
                        | None -> failwith "Story missing title"
                        | Some t -> 
                            match descendants with 
                            | None -> failwith "Story missing descendants"
                            | Some commentCount ->
                                match score with
                                | None -> failwith "Story missing score"
                                | Some s -> Story (metadata, t, url, commentCount, s, text)
                    | "comment" ->
                        match text with 
                        | None -> failwith "Comment missing text"
                        | Some commentText ->
                            match parent with
                            | None -> failwith "Comment missing parent"
                            | Some parentId -> Comment (metadata, parentId, commentText)
                    | "poll" -> 
                        match title with
                        | None -> failwith "Poll missing title"
                        | Some t -> 
                            match text with
                            | None -> failwith "Poll missing text"
                            | Some pollText ->
                                match descendants with
                                | None -> failwith "Poll missing descendants"
                                | Some commentCount -> 
                                    match parts with
                                    | None -> failwith "Poll missing parts"
                                    | Some pollOptions -> Poll (metadata, t, commentCount, pollOptions, pollText)
                    | "pollopt" ->
                        match parent with
                        | None -> failwith "Poll option missing parent"
                        | Some parentId ->
                            match text with 
                            | None -> failwith "PollOpt missing text"
                            | Some pollText -> 
                                match score with
                                | None -> failwith "PollOpt missing score"
                                | Some s -> PollOpt (metadata, parentId, pollText, s)
                    | _ -> failwith "Unknown type"
            <!> Json.read "id"
            <*> Json.tryRead "deleted"
            <*> Json.read "type"
            <*> Json.read "by"
            <*> Json.read "time"
            <*> Json.tryRead "text"
            <*> Json.tryRead "dead"
            <*> Json.tryRead "parent"
            <*> Json.tryRead "kids"
            <*> Json.tryRead "url"
            <*> Json.tryRead "score"
            <*> Json.tryRead "title"
            <*> Json.tryRead "parts"
            <*> Json.tryRead "descendants"

    type HackerNewsUser = {
        Id : string
        Delay : int option
        Created : System.DateTime
        Karma : int
        About : string option
        Submitted : int array option
    } 
    with
        static member FromJson (_:HackerNewsUser) =
                fun id delay created karma about submitted ->
                    { Id = id; Delay = delay; Created = created |> fromUnixTime; Karma = karma; About = about; Submitted = submitted }
            <!> Json.read "id"
            <*> Json.tryRead "delay"
            <*> Json.read "created"
            <*> Json.read "karma"
            <*> Json.tryRead "about"
            <*> Json.tryRead "submitted"

    let httpGet uri =
        async {
            let req = System.Net.HttpWebRequest.Create (System.Uri (uri)) :?> System.Net.HttpWebRequest
            let! res = req.AsyncGetResponse ()
            let stm = res.GetResponseStream ()
            use rdr = new System.IO.StreamReader(stm)
            let! content = rdr.ReadToEndAsync () |> Async.AwaitTask
            return content
        }

    let newsItem id = 
        async {
            let uri = sprintf "%s/item/%i.json" BaseUri id
            let! contents = httpGet uri
            let (news:HackerNewsItem) = contents |> Json.parse |> Json.deserialize 
            return news
        }

    let topNews () = 
        async {
            let uri = sprintf "%s/topstories.json" BaseUri 
            let! contents = httpGet uri
            let (newsIds:int array) = contents |> Json.parse |> Json.deserialize 
            let! (news:HackerNewsItem array) = 
                newsIds |> Array.take 20 |> Array.map(fun id -> 
                    async { 
                        let! item = newsItem id
                        return item
                    })
                |> Async.Parallel
            return news
        }

