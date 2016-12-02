namespace SuaveChironNetcoreTests

open Chiron.Parsing
open Chiron.Mapping
open Xunit
open SuaveChironNetcore.HNProvider

module HNProviderTests =

    type Tests() =

        let sampleStory = """{
            "by" : "dhouston",
            "descendants" : 71,
            "id" : 8863,
            "kids" : [ 8952, 9224, 8917, 8884, 8887, 8943, 8869, 8958, 9005, 9671, 8940, 9067, 8908, 9055, 8865, 8881, 8872, 8873, 8955, 10403, 8903, 8928, 9125, 8998, 8901, 8902, 8907, 8894, 8878, 8870, 8980, 8934, 8876 ],
            "score" : 111,
            "time" : 1175714200,
            "title" : "My YC app: Dropbox - Throw away your USB drive",
            "type" : "story",
            "url" : "http://www.getdropbox.com/u/2/screencast.html"
            }"""

        let sampleComment = """{
            "by" : "norvig",
            "id" : 2921983,
            "kids" : [ 2922097, 2922429, 2924562, 2922709, 2922573, 2922140, 2922141 ],
            "parent" : 2921506,
            "text" : "Aw shucks, guys ... you make me blush with your compliments.<p>Tell you what, Ill make a deal: I'll keep writing if you keep reading. K?",
            "time" : 1314211127,
            "type" : "comment"
            }"""

        let sampleJob = """{
            "by" : "justin",
            "id" : 192327,
            "score" : 6,
            "text" : "Justin.tv is the biggest live video site online. We serve hundreds of thousands of video streams a day, and have supported up to 50k live concurrent viewers. Our site is growing every week, and we just added a 10 gbps line to our colo. Our unique visitors are up 900% since January.<p>There are a lot of pieces that fit together to make Justin.tv work: our video cluster, IRC server, our web app, and our monitoring and search services, to name a few. A lot of our website is dependent on Flash, and we're looking for talented Flash Engineers who know AS2 and AS3 very well who want to be leaders in the development of our Flash.<p>Responsibilities<p><pre><code>    * Contribute to product design and implementation discussions\n    * Implement projects from the idea phase to production\n    * Test and iterate code before and after production release \n</code></pre>\nQualifications<p><pre><code>    * You should know AS2, AS3, and maybe a little be of Flex.\n    * Experience building web applications.\n    * A strong desire to work on website with passionate users and ideas for how to improve it.\n    * Experience hacking video streams, python, Twisted or rails all a plus.\n</code></pre>\nWhile we're growing rapidly, Justin.tv is still a small, technology focused company, built by hackers for hackers. Seven of our ten person team are engineers or designers. We believe in rapid development, and push out new code releases every week. We're based in a beautiful office in the SOMA district of SF, one block from the caltrain station. If you want a fun job hacking on code that will touch a lot of people, JTV is for you.<p>Note: You must be physically present in SF to work for JTV. Completing the technical problem at <a href=\"http://www.justin.tv/problems/bml\" rel=\"nofollow\">http://www.justin.tv/problems/bml</a> will go a long way with us. Cheers!",
            "time" : 1210981217,
            "title" : "Justin.tv is looking for a Lead Flash Engineer!",
            "type" : "job",
            "url" : ""
            }"""

        let samplePoll = """{
            "by" : "pg",
            "descendants" : 54,
            "id" : 126809,
            "kids" : [ 126822, 126823, 126993, 126824, 126934, 127411, 126888, 127681, 126818, 126816, 126854, 127095, 126861, 127313, 127299, 126859, 126852, 126882, 126832, 127072, 127217, 126889, 127535, 126917, 126875 ],
            "parts" : [ 126810, 126811, 126812 ],
            "score" : 46,
            "text" : "",
            "time" : 1204403652,
            "title" : "Poll: What would happen if News.YC had explicit support for polls?",
            "type" : "poll"
            }"""

        let samplePollOpt = """{
            "by" : "pg",
            "id" : 160705,
            "parent" : 160704,
            "score" : 335,
            "text" : "Yes, ban them; I'm tired of seeing Valleywag stories on News.YC.",
            "time" : 1207886576,
            "type" : "pollopt"
            }"""

        [<Fact>]
        member x.ParseSampleJob () =
            let (newsItem:HackerNewsItem) = sampleJob |> Json.parse |> Json.deserialize
            match newsItem with
            | Job(metadata, title) -> Assert.Equal("Justin.tv is looking for a Lead Flash Engineer!", title)
            | _ -> failwith "News item should be Job type."

        [<Fact>]
        member x.ParseSampleComment () =
            let (newsItem:HackerNewsItem) = sampleComment |> Json.parse |> Json.deserialize
            match newsItem with
            | Comment(metadata, parentId, text) -> Assert.Equal("Aw shucks, guys ... you make me blush with your compliments.<p>Tell you what, Ill make a deal: I'll keep writing if you keep reading. K?", text)
            | _ -> failwith "News item should be Comment type."

        [<Fact>]
        member x.ParseSampleStory () =
            let (newsItem:HackerNewsItem) = sampleStory |> Json.parse |> Json.deserialize
            match newsItem with
            | Story(metadata, title, url, commentCount, score, text) -> Assert.Equal("My YC app: Dropbox - Throw away your USB drive", title)
            | _ -> failwith "News item should be Story type."

        [<Fact>]
        member x.ParseSamplePollTest () =
            let (newsItem:HackerNewsItem) = samplePoll |> Json.parse |> Json.deserialize
            match newsItem with
            | Poll(metadata, title, commentCount, pollOps, text) -> Assert.Equal("Poll: What would happen if News.YC had explicit support for polls?", title)
            | _ -> failwith "News item should be Poll type."

        [<Fact>]
        member x.ParseSamplePollOpt () =
            let (newsItem:HackerNewsItem) = samplePollOpt |> Json.parse |> Json.deserialize
            match newsItem with
            | PollOpt(metadata, parentId, text, votes) -> Assert.Equal("Yes, ban them; I'm tired of seeing Valleywag stories on News.YC.", text)
            | _ -> failwith "News item should be PollOpt type."
