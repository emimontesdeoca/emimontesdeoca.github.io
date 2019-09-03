---
layout: post
title: "My take on in-memory cache"
description: "My way of handling memory cache"
comments: true
keywords: "tutorial"
---

I've been working on some stuff that handled a big amount of data. While managing the data I realized that some of it never changes, or at least for some fixed time, it doesn't.

So I thought that it would be useful to create a personal cache repository, of course this isn't new, a few weeks ago I read about this in StackOverflow's [post](https://nickcraver.com/blog/2019/08/06/stack-overflow-how-we-do-app-caching/#in-memory--redis-cache) written by [Nick Craver](https://nickcraver.com/) about how they manage application cache.

Also I've always wanted to work with cache, so why not.

## Flow

[![Image from Gyazo](https://i.gyazo.com/830d5a91089c3344c8b406c66ea547b8.png)](https://gyazo.com/830d5a91089c3344c8b406c66ea547b8)

## Implementation

### Before we start

I know, I know. There's [System.Runtime.Caching](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.caching.memorycache?view=netframework-4.8) that handles memory cache. But I decided to build it myself, if you want to use that class, check [here](https://stackoverflow.com/search?q=System.Runtime.Caching) for a how-to.

### CacheItem

The first step is to have a class that stores the value of an object and the expiration date. Probably there's a better way to do it, but this is what I thought, so there you go:

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

### CacheRepository

Then we need a class that handles the objects and saves them somewhere (as `CacheItem`) . I like to handle all the data/models in classes that has the sufix  `Repository`, but you dont have to, so let's build one.

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

So far we have a static dictionary called `Cache` where all the items will be stored. Remember that this will only last while the application is running, thus in-memory caching.

Also keep in mind that the `Cache` item will be initialized once the `CacheRepository` class is loadad.

The only method available when invoking the CacheRepository class is `GetOrSet(string key, Func<T> lookup, TimeSpan durationMinutes)` that needs three parameters:

1. `key`: the identifier of the object to save.
2. `lookup`: the callback function in case the cache expired or it's null.
3. `durationMinutes`: the duration in minutes which will be added to the current datetime.

## Time to cache

Now, use our caching repository to get some data from somewhere. In order for all of this to make sense, let's create an example object with some properties, and then a repository to fetch and fill a list of that object.

```csharp
public class User 
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}

public class UserRepository 
{
    public List<User> Get()
    {
        // Code to get users
    }
}
```

Since we have the method to fill a list of `User`, let's make use of the `CacheRepository` class.

```csharp
private UserRepository _userRepository = new UserRepository();

private List<User> _user;

public List<User> User
{
    get
    {
        _user = CacheRepository.GetOrSet($"users", usersRepo.Get, TimeSpan.FromMinutes(10));
        return _user;
    }
}
```

And just like that, everytime that you access the variable `User`, it will ask to `CacheRepository` for the value of an object that has the key `users`.

If that key exists, it will check the expiration date. If either one of these conditions are false, it will use tha callback to set the value (with `usersRepo.Get`) of the object, save it to cache with the expiration date set to `DateTime.UtcNow + TimeSpan.FromMinutes(10)` and return it.

