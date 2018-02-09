﻿using ServiceStack;
using ServiceStack.Templates;
using ServiceStack.DataAnnotations;
using TechStacks.ServiceModel;

namespace TechStacks.ServiceInterface
{
    [Exclude(Feature.Metadata)]
    [FallbackRoute("/{PathInfo*}", Matches = "AcceptsHtml")]
    public class FallbackForClientRoutes
    {
        public string PathInfo { get; set; }
        public ResponseStatus ResponseStatus { get; set; }
    }

    [Route("/ping")]
    public class Ping {}
    
    public class ClientRoutesService : Service
    {
        //Return index.html for unmatched requests so routing is handled on client
        public object Any(FallbackForClientRoutes request) => 
            new PageResult(Request.GetPage("/"));

        public object Any(Ping request) => "OK";
    }

    //Client Routes to generate urls in sitemap.xml
    [Route("/tech")]
    public class ClientAllTechnologies {}

    [Route("/tech/{Slug}")]
    public class ClientTechnology
    {
        public string Slug { get; set; }
    }

    [Route("/stacks")]
    public class ClientAllTechnologyStacks { }

    [Route("/{Slug}")]
    public class ClientTechnologyStack
    {
        public string Slug { get; set; }
    }

    [Route("/users/{UserName}")]
    public class ClientUser
    {
        public string UserName { get; set; }
    }
}