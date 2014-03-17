<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<ErlangVMA.LoginModel>" MasterPageFile="~/Views/Master.master" %>
<%@ Import Namespace="System.Web.Security" %>

<asp:Content ContentPlaceHolderID="PageTitle" runat="server">
    Login
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
  <form id="loginForm" runat="server">
    <h3>
      Login
    </h3>
      
    <table>
      <tr>
        <td><%= Html.LabelFor(model => model.Name) %>:</td>
        <td>
          <%= Html.EditorFor(model => model.Name) %>
        </td>
        <td><%= Html.ValidationMessageFor(model => model.Name) %></td>
      </tr>
      <tr>
        <td><%= Html.LabelFor(model => model.Password) %>:</td>
        <td>
          <%= Html.PasswordFor(model => model.Password) %>
        </td>
        <td><%= Html.ValidationMessageFor(model => model.Password) %></td>
      </tr>
      <tr>
        <td><%= Html.LabelFor(model => model.Remember) %>:</td>
        <td>
          <%= Html.EditorFor(model => model.Remember) %>
        </td>
      </tr>
    </table>
    
    <input type="submit" value="Login" name="Login" />
  </form>
</asp:Content>