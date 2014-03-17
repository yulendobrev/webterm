<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage" MasterPageFile="~/Views/Master.master" %>

<asp:Content ContentPlaceHolderID="PageTitle" runat="server">
	Virtual Machine
</asp:Content>

<asp:Content ContentPlaceHolderID="AdditionalHead" runat="server">
	<script type="text/javascript" src="~/Scripts/jquery-2.0.3.js"></script>
	<script type="text/javascript" src="~/Scripts/jquery.signalR-2.0.2.js"></script>
	<script type="text/javascript" src="~/signalr/hubs"></script>
	<script type="text/javascript" src="~/Scripts/vmconsole.js"></script>
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
	<div id="vmConsole" style="width: 100%">
		<textarea id="vmConsoleArea" rows="25"></textarea>
		<div id="vmConsoleDisplay"></div>
	</div>
</asp:Content>