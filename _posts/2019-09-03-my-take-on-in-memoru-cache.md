---
layout: post
title: "My take on memory cache"
description: "My way of handling memory cache"
comments: true
keywords: "tutorial"
---

I've been working on some stuff that handled a big singnificar amount of data. While managing the data I realized that some of it never changes, or at least for some fixed time, it doesn't.

So I thought that it would be useful to create a personal cache repository, of course this isn't new, I read about this in StackOverflow's [post](https://nickcraver.com/blog/2019/08/06/stack-overflow-how-we-do-app-caching/#in-memory--redis-cache) about how they manage application cache.


## Flow

## Implementation

It's pretty simple, we need an object that identifies the object which also has a property that stores the expiration date.

```csharp

 public class CacheItem
    {
        public string Identifier { get; set; }
        public object Value { get; set; }
        public DateTime ValidUntil { get; set; }

        public CacheItem(string identifier, object value, TimeSpan valid)
        {
            Identifier = identifier;
            Value = value;
            ValidUntil = DateTime.Now.Add(valid);
        }
    }

```

Then we need a class that handles the objects and saves them somewhere (as `CacheItem`) . I like to handle all the data/models in classes that has the  sufix  `Repository`, so let's build one.

```csharp

 public class CacheRepository
    {
        private static Dictionary<string, string> Cache = new Dictionary<string, string>();

        private static T Set<T>(string key, Func<T> lookup, TimeSpan durationMinutes)
        {
            var item = new Models.Item(key, lookup(), durationMinutes);
            return Save<T>(key, item);
        }

        private static T Save<T>(string key, Item item)
        {
            Cache[key] = JsonConvert.SerializeObject(item);
            return (T)item.Value;
        }

        private static T Get<T>(string key)
        {
            var cached = Cache.FirstOrDefault(x => x.Key == key).Value;
            var item = string.IsNullOrEmpty(cached) ? null : JsonConvert.DeserializeObject<Item>(cached);
            return (item == null) 
            ? default : (item.ValidUntil > DateTime.UtcNow) 
            ? JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(item.Value)) : default;
        }

        public static T GetOrSet<T>(string key, Func<T> lookup, TimeSpan durationMinutes)
        {
            var cache = Get<T>(key);
            return cache == null ? Set(key, lookup, durationMinutes) : cache;
        }
    }

```

So far we have a static property called `Cache` where all the items will be stored. Remember that this will only last while the application is running, thus in-memory caching.

The only method available when invoking the CacheRepository class is `GetOrSet(string key, Func<T> lookup, TimeSpan durationMinutes)` that needs three parameters:

1. key: the identifier of the object to save.
2. lookup: the callback function in case the cache expired or it's null.
3. durationMinutes: the duration in minutes which will be added to the current datetime.