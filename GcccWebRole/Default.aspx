<%@ Page Title="Home Page" Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="GcccWebRole._Default" %>

<!DOCTYPE html>

<html>
    <body>
        <form id="form1" runat="server">
            <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>

            <label for="NumeLabel">Nume: </label> <asp:TextBox ID="NameTB" runat="server" /> 
            <asp:RequiredFieldValidator ID="NameRFV" runat="server" ControlToValidate="NameTB" Text="*" />

            <br />
            <label for="MesajLabel">Mesaj: </label> <asp:TextBox ID="MesajTB" runat="server" TextMode="MultiLine" Rows="4" />
            
            <br />
            <label for="ImagineLabel">Imagine:</label> <asp:FileUpload ID="ImagineFU" runat="server" />
            
            <br />
            <br />

            <asp:Button ID="SubmitBtn" Text="Submit" OnClick="SubmitBtn_Click" runat="server" />
            
            <br />
            <br />
            <br />

            <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                <ContentTemplate>
                    Comentarii:<br />
                    <asp:DataList ID="ComentariiDataList" runat="server" DataSourceID="ComentariiDataSource" BorderColor="Black" CellPadding="5" CellSpacing="5" RepeatDirection="Vertical" RepeatLayout="Flow" RepeatColumns="0" BorderWidth="1">
                        <ItemTemplate>
                            <asp:ImageButton ID="Imagine" ImageUrl='<%# DataBinder.Eval(Container.DataItem, "ThumbnailUrl") %>' CausesValidation="false" runat="server" ImageFull='<%# DataBinder.Eval(Container.DataItem, "ImageUrl") %>' OnClick="Imagine_Click" />
                            <p> <%# DataBinder.Eval(Container.DataItem, "Message") %> </p> <br />
                            <strong> <%# DataBinder.Eval(Container.DataItem, "GuestName") %> </strong>
                        </ItemTemplate>
                    </asp:DataList>
                    <asp:Timer ID="Timer1" runat="server" Interval="15000" OnTick="Timer1_Tick"></asp:Timer>
                </ContentTemplate>
            </asp:UpdatePanel>
            <asp:ObjectDataSource ID="ComentariiDataSource" runat="server" DataObjectTypeName="GcccData.Entry" SelectMethod="GetEntries" TypeName="GcccData.DataSource"></asp:ObjectDataSource>

            <div id="imageMod">
                <asp:UpdatePanel ID="ImagineUPMod" runat="server" ChildrenAsTriggers="false" UpdateMode="Conditional">
                    <ContentTemplate>
                        <asp:Image ID="ImagineFull" runat="server" />
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>
        </form>
    </body>
</html>
