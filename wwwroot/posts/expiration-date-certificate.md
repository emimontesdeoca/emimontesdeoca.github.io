After working on the Wordlzor game that I recently did, I needed to add a pretty simple functionality: focus the entire game when entering. 

This needed to be done because the user could actually type on the game and not only use the on screen keyboard.

## Javascript file

In order to do this, we have to create a Javascript file called `app.js` that will hold a function

```js
window.FocusElement = (element) => {
    element.focus();
};
```

After we have done that, we need to include the script in the `index.html` file:

```html
<script src="js/app.js"></script>
```

## Blazor component

Once we have initialized the script, we need to find an element to focus on, so in any of our components, we need to reference that element to an object.

So in our blazor component let's add a div wit the `
@ref` property:

```html
<div @ref=@elementToFocus tabindex="0" >
    This is my focus component
</div>
```

Then on the component logic you can have the property as an `ElementReference`:

```csharp
/// <summary>
/// Reference to element
/// </summary>
private ElementReference elementToFocus;
```

## Injecting JSInterop

Now, in order to call the function that we have in our Javascript file, we need to use `JSInterop`.

First of all we must inject it on component with the following syntax:

```csharp
[Inject]
public IJSRuntime JSRuntime { get; set; }
```

With the service injected, we now are able to call any of the methods that it have, like `InvokeVoidAsync`, which will call the function:

```csharp
public async Task Focus()
{
    // Focus when initializing
    await JSRuntime.InvokeVoidAsync("window.FocusElement", elementToFocus);
}
```

And when you call the funciton `Focus()`, it will focus the elemetn that we created, obviously you can refactor and pass the element itself as a parameter.

If you want to take a look at how I implemented it, take a look at the [Wordlzor](https://github.com/emimontesdeoca/Wordlzor) source code, I used it for alerts and focusing when closing the instruction modal.
