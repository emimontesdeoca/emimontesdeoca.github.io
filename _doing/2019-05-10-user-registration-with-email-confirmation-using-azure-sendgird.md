---
layout: post
title: "User registration with email confirmation using Azure SendGrid"
description: "Getting to know Azure SendGird with a real world case"
comments: true
keywords: "csharp"
---
# Introduction

If you have used the official `SmtpClient` class you should already know that it is obsolete which is what it says in the [Microsoft's official documentation](https://docs.microsoft.com/en-us/dotnet/api/system.net.mail.smtpclient?view=netcore-2.2). And the new king is [Mailkit](https://github.com/jstedfast/MailKit).

The current generation is all cloud, and since I'm a newbie with Azure I heard about [SendGrid with Azure](https://docs.microsoft.com/es-es/azure/sendgrid-dotnet-how-to-send-email) while I was attending Azure Bootcamp in Madrid last month.

So in order to get some knowledge of how this new framework is working let's build real word problem, user registration with confirmation by email. That's a real world problem... right?

## SendGrid

From Microsoft docs official documentation:

> SendGrid is a cloud-based email service that provides reliable transactional email delivery, scalability, and real-time analytics along with flexible APIs that make custom integration easy. Common SendGrid use cases include:
> - Automatically sending receipts or purchase confirmations to customers.
> - Administering distribution lists for sending customers monthly fliers and promotions.
> - Collecting real-time metrics for things like blocked email and customer engagement.
> - Forwarding customer inquiries.
> - Processing incoming emails.
> 
> For more information, visit https://sendgrid.com or SendGrid's C# library GitHub repo.


# Diagram

[![Image from Gyazo](https://i.gyazo.com/9b6058e4e6728bd9281eedb8ec2750b8.png)](https://gyazo.com/9b6058e4e6728bd9281eedb8ec2750b8)

As you can see this is how I do like to structure things, separating the task by server, client or user and database, starting from the top and finishing in the bottom.

This diagram explains a bit of how is the procedure from a user clicking the `register` button, creating the token, sending the confirmation page and finally confirming the registration.

I belive this will be a long but simple tutorial but well, I do like to make things good so have fun while you read it!

We will be using **Azure**, **.NET Core API**, **Azure SQL database** and a few more things!

# Approach

Since I don't really want to build a website, let's build an API which we will be using to POST/GET our data and then let the server side do the magic.



