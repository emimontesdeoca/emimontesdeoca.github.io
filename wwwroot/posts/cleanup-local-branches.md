Have you ever got to a point where you had too many local branches? It happens to me a lot, since we use branches for each feature, bug, or chore.

I end up with something like this and it get's super dirty after a while

<img src="https://imgur.com/W3OGJE7.png">

It really anoys me, so I'll just share a quick command you can run on your console to do this cleanup!

I found this command in Stack Overflow in the following [answer](https://stackoverflow.com/a/56671336/7823470) by [Robert Corvus](https://stackoverflow.com/users/529612/robert-corvus), which is a version that runs on Powerhsell.

**Please be careful when running this command becase you can lose your changes**

Before running it, remember to update the `MY_MASTER_BRANCH_NAME` to your main branch, which can be `master` like I use or the new ones that come by default called `main`.

``` powershell
git branch | %{ $_.Trim() } | ?{ $_ -ne 'MY_MASTER_BRANCH_NAME' } | %{ git branch -D $_ }

```

After you run this command, you will get an output like this

<img src="https://imgur.com/VJn89OZ.png">

Hope it's helpful to you!