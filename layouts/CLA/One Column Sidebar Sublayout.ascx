<%@ register TagPrefix="sc" Namespace="Sitecore.Web.UI.WebControls" Assembly="Sitecore.Kernel" %>
<div style="width:100%;min-height:400px;">
	<div style="background-color:#FFFFFF;float:left;width:719px;min-height:400px;margin-right:5px;">
		<div style="width:690px;padding-top:15px;padding-left:15px;padding-bottom:15px;padding-right:10px;">
			<h1>
				<sc:fieldrenderer runat="server" renderingid="{E1AF4AA3-3B5D-4611-8C71-959AD261E5B7}" FieldName="Title">
			</sc:fieldrenderer>
			</h1>
			<p>
				<sc:fieldrenderer runat="server" renderingid="{E1AF4AA3-3B5D-4611-8C71-959AD261E5B7}" FieldName="Body">
			</sc:fieldrenderer>
			</p>
		</div>
	</div>
	<div style="background-color:#FFFFFF;float:right;min-height:400px;width:298px;border:solid 1px #7A7A7A;">
		<div style="background-color:#D2391B;height:50px;width:100%">
			
			 
		</div>
		<div>
			<sc:placeholder runat="server" key="sidebar"></sc:placeholder>
		</div>
	</div>
	<div style="clear:both">
	</div>
</div>