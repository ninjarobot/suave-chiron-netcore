namespace SuaveChironNetcore
(* 
    The Program module should deal with interoperability with the OS,
    stdin, stdout, environment variables, etc.  It should compose the
    functions into a working application.
*)

open System

// Most applications will have multiple modules and use a namespace.
module Program =

    [<EntryPoint>]
    let main argv = 
        WebApi.startServer ()
        0 // return an integer exit code
