# Spidey

[![.NET Publish](https://github.com/JaCraig/Spidey/actions/workflows/dotnet-publish.yml/badge.svg)](https://github.com/JaCraig/Spidey/actions/workflows/dotnet-publish.yml)

Library to help with crawling web content. Compatible with .Net Core and .Net Framework.

## Setting up the Library

Spidey needs to be added to your app's service collection in order to be wired up properly. This is done by calling the RegisterSpidey extension method on the ServiceCollection object. This is done like so:

    new ServiceCollection().RegisterSpidey();

Or if you're using [Canister](https://github.com/JaCraig/Canister):

	new ServiceCollection().AddCanisterModules();

The RegisterSpidey function is an extension method that registers it with the IoC container. When this is done, Spidey is ready to use.

## Basic Usage

Spidey really boils down to using one class called Crawler:

    ServiceCollection.AddSingleton(new Options
				{
				    ItemFound = FoundFile=>{}                                            //The callback method used when a new page is discovered.
				    Allow = new List<string> { "http://mywebsite", "http://mywebsite2" },    //Regexes of what sites/pages are allowed to be crawled.
				    FollowOnly = new List<string> { "..." },                                 //Regexes of pages to only follow links that are found on them.
				    Ignore = new List<string> { "..." },                                     //Regexes that the system will ignore when they are encountered.
				    StartLocations = new List<string> { "http://mywebsite", "http://mywebsite2" },    //Starting URLs for the crawler.
				    UrlReplacements = new Dictionary<string,string> {...}                    //When the system hits one of the keys in the dictionary, it will replace it with the value.
				});
				
Note that it is recommended that you actually register the Options object in your ServiceCollection and resolve the Crawler object from the service provider but it is not required. You can simply new up an instance of Crawler if you want. Anyway, the Options class has a number of properties, some of which are not displayed above such as NetworkCredentials, UseDefaultCredentials, and Proxy. The callback method is what will be called by the system once a link's info has been received and looks like this:

    void CallbackMethod(ResultFile obj) { ... }
	
The library will handle parsing of links found within the page, downloading the content, etc. for the most part. At this point all you have to do is call the StartCrawl method:

    MyCrawler.StartCrawl();
	
## Customization
	
Note that it's possible to customize the crawler's various parts. The system is divided into the following sections:

1. Content Parser (IContentParser) - This parses the resulting data and converts it to the ResultFile object.
2. Engine (IEngine) - This downloads the content from the server.
3. Link Discoverer (ILinkDiscoverer) - Takes the content from the engine and looks for links to other resources.
4. Processor (IProcessor) - Takes the parsed content and hands it off to your code. The default one simply calls the method provided in the options.
5. Scheduler (IScheduler) - Handles handing out work to the various workers.
6. Pipeline (IPipeline) - Manages the various parts of the process by feeding the content to the next bit of the process.

These subsystems all implement interfaces found in the Spidey.Engines.Interfaces namespace. In order to replace the default in any of these systems all you need to do is create a class that implements the interface that you want to replace. After that the system will automatically pick it up if resolved from the service provider. If you, instead, new up a Crawler object then you will need to compose the Pipeline object.

## Installation

The library is available via Nuget with the package name "Spidey". To install it run the following command in the Package Manager Console:

Install-Package Spidey

## FAQ

1. Is it possible to run the crawler using multiple nodes?

The default scheduler assumes that you are only running the crawler from one location and doesn't talk to other instances of the application. But it is possible to replace the scheduler with one that will talk via some mechanism like a database to coordinate work between instances and is recommended for more complex setups.

## Build Process

In order to build the library you will require the following:

1. Visual Studio 2022

Other than that, just clone the project and you should be able to load the solution and build without too much effort.
