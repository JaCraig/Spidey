# Spidey

[![Build status](https://ci.appveyor.com/api/projects/status/0derh6adccmnc8py?svg=true)](https://ci.appveyor.com/project/JaCraig/spidey)

Library to help with crawling web content. Compatible with .Net Core and .Net Framework.

## Setting up the Library

Spidey relies on [Canister](https://github.com/JaCraig/Canister) in order to hook itself up. In order for this to work, you must do the following at startup:

    Canister.Builder.CreateContainer(new List<ServiceDescriptor>())
                    .RegisterSpidey()
                    .Build();

The RegisterSpidey function is an extension method that registers it with the IoC container. When this is done, Spidey is ready to use.

## Basic Usage

Spidey really boils down to using one class called Crawler:

    var MyCrawler = new Crawler(CallbackMethod,
	                        new Options
				{
				    Allow = new List<string> { "http://mywebsite", "http://mywebsite2" },    //Regexes of what sites/pages are allowed to be crawled.
				    FollowOnly = new List<string> { "..." },                                 //Regexes of pages to only follow links that are found on them.
				    Ignore = new List<string> { "..." },                                     //Regexes that the system will ignore when they are encountered.
				    StartLocations = new List<string> { "http://mywebsite", "http://mywebsite2" },    //Starting URLs for the crawler.
				    UrlReplacements = new Dictionary<string,string> {...}                    //When the system hits one of the keys in the dictionary, it will replace it with the value.
				},
				new DefaultEngine(),
				Serilog.Log.Logger);
								
Starting at the bottom, Serilog is not required to be set up. Passing in null here will just mean that the crawler will eat any logging info. The DefaultEngine is the IEngine class that is built into the system. This is the class that the system uses to download the content. You can specify your own engine here as needed. The Options class has a number of properties, some of which are not displayed above such as NetworkCredentials, UseDefaultCredentials, and Proxy. These can be used to specify network credentials and proxy settings.  The CallbackMethod is what will be called by the system once a links info has been received and looks like this:

    private static void CallbackMethod(ResultFile obj) { ... }
	
The library will handle parsing of links found within the page, downloading the content, etc. for the most part. At this point all you have to do is call the StartCrawl method:

    MyCrawler.StartCrawl();

## Installation

The library is available via Nuget with the package name "Spidey". To install it run the following command in the Package Manager Console:

Install-Package Spidey

## Build Process

In order to build the library you will require the following:

1. Visual Studio 2017

Other than that, just clone the project and you should be able to load the solution and build without too much effort.
