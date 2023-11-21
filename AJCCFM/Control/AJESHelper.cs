using AJCCFM.Models;
using Core.Domain;


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace AJCCFM.Control
{
    public static class AJESHelper
    {
        public static MvcHtmlString EmployeeCode(this HtmlHelper htmlhelper, string EmpCode)
        {


            string rawHtml;
            Table tt = new Table();
            TableRow _tr;
            TableCell _tc;

            _tr = new TableRow();
            _tc = new TableCell();
            HtmlInputText txtEmpcode;
            HtmlInputButton btnEmpCode;
            _tc.Text = "Employee Code";
            _tr.Controls.Add(_tc);

            _tc = new TableCell();
            txtEmpcode = new HtmlInputText();
            txtEmpcode.Value = EmpCode;
            txtEmpcode.Attributes["class"] = "form-control";
            txtEmpcode.ID = "AJESEmpCode";
            _tc.Controls.Add(txtEmpcode);
            _tr.Controls.Add(_tc);

            _tc = new TableCell();
            btnEmpCode = new HtmlInputSubmit();
            btnEmpCode.Value = "Get Employee Infomation";
            btnEmpCode.Attributes["class"] = "btn btn-danger";
            _tc.Controls.Add(btnEmpCode);
            _tr.Controls.Add(_tc);
            tt.Controls.Add(_tr);

            using (TextWriter stringWriter = new StringWriter())
            {

                using (System.Web.UI.HtmlTextWriter renderOnMe = new HtmlTextWriter(stringWriter))
                {
                    // now render the control inside the htm writer
                    tt.RenderControl(renderOnMe);
                    // here is your control rendered output.
                    rawHtml = stringWriter.ToString();
                    var strTemplate = MvcHtmlString.Create(rawHtml);
                    //return strTemplate;
                }
            }
            return new MvcHtmlString(rawHtml);



        }


        public static MvcHtmlString AJESADDropdownList(this HtmlHelper helper, string name, IEnumerable<UserDetail> list, string ForemenCode)

        {

            //Creating a select element using TagBuilder class which will create a dropdown.

            TagBuilder dropdown = new TagBuilder("select");

            //Setting the name and id attribute with name parameter passed to this method.

            dropdown.Attributes.Add("name", name);

            dropdown.Attributes.Add("id", name);

            dropdown.Attributes["class"] = "form-control";

            //Created StringBuilder object to store option data fetched oen by one from list.

            // StringBuilder options = new StringBuilder();

            // StringBuilder options1 = new StringBuilder();

            //Iterated over the IEnumerable list.

            string options = "";



            foreach (var item in list)

            {

                //Each option represents a value in dropdown. For each element in the list, option element is created and appended to the stringBuilder object.

                // options = options.Append("<option value='" + item.ipPhone + "' >" + item.DisplayText + "</option>");





                options = options + "<OPTION";

                options = options + " value='" + item.LoginName + "'";

                if (ForemenCode == item.ipPhone)

                {

                    options = options + "selected";

                }

                options = options + ">" + item.DisplayText + "</OPTION>";

            }







            //assigned all the options to the dropdown using innerHTML property.



            dropdown.InnerHtml = options.ToString();

            //Assigning the attributes passed as a htmlAttributes object.

            //dropdown.MergeAttributes(new RouteValueDictionary(htmlAttributes));

            //Returning the entire select or dropdown control in HTMLString format.

            return MvcHtmlString.Create(dropdown.ToString(TagRenderMode.Normal));

        }
    }
}