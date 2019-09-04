---
layout: post
title: "A case of success: safely accessing a local only SQL Server with Azure Relay"
description: "Accessing safely to a local only SQL Server with Microsoft Azure Relay"
comments: true
keywords: "tutorial"
---

Some time ago I had to work with a company that had their database server without internet connection, they main reason for it i that they were dealing with sensitive information and that database was being accessed only from that network. No one was accessing that database from outside.

We needed something to access that database from outside that meet this requirementes:
 - Safe
 - Reliable
 - Fast

Note that, we won't be accessing that database server directly, we will be something like a client asking for data in the same network, but instead of using it there, we will be sending that information outside.

## Why not a WebAPI?

We could have installed an IIS and make an API project, but that means more requirements for the client, keep in mind that in the case that we wanted to use a webAPI, we had to user good enough hardware for it to handle. 

So that's why we thought that a webAPI would too overkill for what the client needs. 

## Relay

Microsofts offers a solution to this called [Azure Service Bus Relay](https://docs.microsoft.com/en-US/azure/service-bus-relay/), which we will be using to make a solution.

A quick introduction of what Azure Relay is:

>The Azure Relay service enables you to securely expose services that run in your corporate network to the public cloud. You can do so without opening a port on your firewall, or making intrusive changes to your corporate network infrastructure.
>
>The relay service supports the following scenarios between on-premises services and applications running in the cloud or in another on-premises environment.
> * Traditional one-way, request/response, and peer-to-peer communication
> * Event distribution at internet-scope to enable publish/subscribe scenarios
> * Bi-directional and unbuffered socket communication across network boundaries.
> * Azure Relay differs from network-level integration technologies such as VPN. An Azure relay can be scoped to a single application endpoint on a single machine. The VPN technology is far more intrusive, as it relies on altering the network environment.

## Diagram

So far, this is current strucuture of our client

[![Image from Gyazo](https://i.gyazo.com/c4fedebc9dbfe585a770edca0972af52.png)](https://gyazo.com/c4fedebc9dbfe585a770edca0972af52)

As you can see, they have a local network with devices and a server that can access WAN, but the server with the SQL Server is blocked to inbound and outbound connections from WAN and only can be accessed from LAN.

Using Microsoft Azure Relay we will implement a receiver in the local network that will open us the SQL Server, and then an app that can send queries to that receiver.

## Microsoft Azure Relay

As stated in the [Microsoft Azure Relay](https://docs.microsoft.com/en-US/azure/service-bus-relay/):

> Azure Relay service facilitates hybrid applications between on-premises and cloud environments within a corporate enterprise network and the public cloud, without having to open a firewall connection or require intrusive changes to a corporate network infrastructure.

## Roadmap

1. Create a Relay namespace by using the Azure portal.
2. Create a hybrid connection in that namespace by using the Azure portal.
3. Write a server (listener) console application to receive messages.
4. Write a client (sender) console application to send messages.
5. Run applications.

### 

In order to get the 

