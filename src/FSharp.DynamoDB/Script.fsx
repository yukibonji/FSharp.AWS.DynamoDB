﻿#I "../../bin"
#r "AWSSDK.Core.dll"
#r "AWSSDK.DynamoDBv2.dll"
#r "FSharp.DynamoDB.dll"

open System

open Amazon
open Amazon.Util
open Amazon.DynamoDBv2
open Amazon.DynamoDBv2.Model

open FSharp.DynamoDB

let account = AWSCredentialsProfile.LoadFrom("default").Credentials
let ddb = new AmazonDynamoDBClient(account, RegionEndpoint.EUCentral1) :> IAmazonDynamoDB

type Test =
    {
        [<HashKey>]
        HashKey : string
        Value : int
        Value2 : int option
        Values : Set<int>
    }

let table = TableContext.GetTableContext<Test>(ddb, "test", createIfNotExists = true) |> Async.RunSynchronously

let value = { HashKey = "1" ; Value = 40 ; Value2 = None ; Values = Set.empty }

let key = table.PutItemAsync(value) |> Async.RunSynchronously

table.GetItemAsync key |> Async.RunSynchronously
table.PutItemAsync({ value with Value = 0}, <@ fun r -> r.HashKey = "1" && r.Values.Count < 10 @>) |> Async.RunSynchronously

table.UpdateItemAsync(key, <@ fun r -> { r with Values = r.Values + set [42] } @>) |> Async.RunSynchronously


<@ fun (r:Test) -> { r with Value = not r.Value ; HashKey = "xa" } @>