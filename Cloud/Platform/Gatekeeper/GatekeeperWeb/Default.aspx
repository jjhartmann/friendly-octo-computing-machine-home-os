<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true"
    CodeBehind="Default.aspx.cs" Inherits="HomeOS.Cloud.Platform.Gatekeeper.GatekeeperWeb._Default" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
</asp:Content>
<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
  <div>
    <h2>
    This is the HomeOS Gatekeeper!
    </h2>
    <p>
    Current Time (on server):&nbsp
    <asp:Label ID="CurrentTimeLabel" runat="server" />
    </p>
    <p>
    Service Status:&nbsp
    <asp:Label ID="ServiceStatusLabel" runat="server" />
    </p>
  </div>
  <div id="ServiceStats" visible="false" runat="server">
    <p>
    Service Start Time:&nbsp
    <asp:Label ID="ServiceStartTimeLabel" runat="server" />
    <br />
    Number of Registered Home Services:&nbsp
    <asp:Label ID="RegisteredServicesCountLabel" runat="server" />
    <br />
    Number of Active Forwarding Sessions:&nbsp
    <asp:Label ID="ActiveForwardingCount" runat="server" />
    <br />
    </p>
  </div>
</asp:Content>
