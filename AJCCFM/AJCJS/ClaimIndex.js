$(document).ready(function () {
   
    $('.btnAddNew').attr('href', 'javascript://');
    $('.btnAddNewService').attr('href', 'javascript://');
    $('.dsend').attr('href', 'javascript://');
   


    $("#dialogpoaDD")
        .dialog({
            autoOpen: false,
            resizable: false,
            width: 750,
            height: 500,
            modal: true,
            title: 'Folder',
            buttons: [
                {
                    text: "Add",
                    click: function () {
                        ConfirmSubmit();
                        //$(this).dialog("close"); // Refresh Page then no use of close

                    }
                },
                {
                    text: "Close", click: function () {
                       // location.href = "/GroupRequest/Index";
                        $(this).dialog("close");
                    }
                }
            ]

        });


    $("#dialogService")
        .dialog({
            autoOpen: false,
            resizable: false,
            width: 750,
            height: 500,
            modal: true,
            title: 'Services',
            buttons: [
                {
                    text: "Add",
                    click: function () {
                        ConfirmSubmitService();
                        //$(this).dialog("close"); // Refresh Page then no use of close

                    }
                },
                {
                    text: "Close", click: function () {
                        // location.href = "/GroupRequest/Index";
                        $(this).dialog("close");
                    }
                }
            ]

        });

   

    $('.btnAddNew').click
        (
            function () {
          
            $("#divProcessing").show();
                var EmpCode = $('#EmpCode').val();
               
            var url = $('#map').data('request-url');
            
            $.ajax({
                type: "POST",
                url: url,
                data: JSON.stringify({'EmpCode':EmpCode}),
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (data) {
                  
                    if (data.Response != '') {
                        $('#ErrorModal').modal('show');
                        $("#divProcessing").hide();
                        return;
                    }
                    $('#dialogpoaDD').html(data.PartialView);
                   // $("#dialogpoaDD").html(response);
                    $("#dialogpoaDD").dialog('open');
                    $("#divProcessing").hide();
                }
            });

        });

    $('.btnAddNewService').click
        (
            function () {

                $("#divProcessing").show();
                var EmpCode = $('#EmpCode').val();

                var url = $('#map').data('request-url');

                $.ajax({
                    type: "POST",
                    url: url,
                    data: JSON.stringify({ 'EmpCode': EmpCode }),
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    success: function (data) {

                        if (data.Response != '') {
                            $('#ErrorModal').modal('show');
                            $("#divProcessing").hide();
                            return;
                        }
                        $('#dialogService').html(data.PartialView);
                        // $("#dialogpoaDD").html(response);
                        $("#dialogService").dialog('open');
                        $("#divProcessing").hide();
                    }
                });

            });
  

    // Partial View _ListClaimDetail Events
    $('.Edit').click(function () {

        $("#divProcessing").show();
        var nClaimDetailID = $(this).attr('nClaimDetailID');
    
        var _parameters = { 'nClaimHeaderID': $('#ClaimID').val(), 'nClaimDetailID': nClaimDetailID, 'ReqID': $('#CashReqHeaderID').val(), 'CopyClaimID': $('#CopyClaimID').val()};


        $.ajax({
            url: "/Claim/NewClaimLineItem",
            type: "POST",
            data: _parameters,
            beforeSend: function () {
                $('#title').html("please wait......");
                $('#AddPermission').attr("disabled", "disabled");
            },
            success: function (response) {
                $("#dialogpoaDD").html(response);
                $("#dialogpoaDD").dialog('open');
                $("#divProcessing").hide();
            }
        });
    });


   



    $('.ShowProcessInvoice').click(function () {

        var url = $('#ViewInvoice').data('request-url');
        var nClaimDetailID = $(this).attr('nClaimDetailID');
      //  var url = "/Claim/ClaimItemInvoice" + "?nClaimDetailID=" + nClaimDetailID;
        var urlParameter = url + "?nClaimDetailID=" + nClaimDetailID;
        $("#myModalBodyDiv").load(urlParameter, function () {
            $("#myModal").modal("show");
        })


    });


    $('.ShowTemplateInfo').click(function () {
       
        var url = $('#ViewTemplate').data('request-url');
        var nClaimDetailID = $(this).attr('nClaimDetailID');
        var nCateogryID = $(this).attr('nCateogryID');
      //  var url = "/Home/Test" + "?CategoryID=" + nCateogryID;
        var urlParameter = url +"?CategoryID=" + nCateogryID + "&ClaimID=" + nClaimDetailID;
       // alert(urlParameter);
        $.ajax
            (
            {
                type: "GET",
                url: urlParameter,
                dataType: "json",
                data: {},
                success: function (response) {
                    $('#myModalBodyDivTempate').html(response);
                    $("#myModalCateogryTemplate").modal("show");
                }
            }
            );

    });


});

// ClaimIndex  
var ConfirmSubmission = function () {

   
    $("#SubmitApprovalModal").modal("show");
}

var ProcessSubmission = function () {
   
    $("#loaderDiv").show();
    var CompanyID = $('#CompanyID').val();
    var BuID = $('#BuID').val();
    var ClaimID = $('#ClaimID').val();
    var Status = $('#Status').val();
    var url = $('#wf').data('request-url');

    $.ajax({
        type: "POST",
       // url: "/WorkFlow/SubmitForApproval",
        url: url,
        contentType: "application/json; charset=utf-8",
        data: JSON.stringify({ 'Doc_Code': '01','CompanyID': CompanyID, 'BuID': BuID, 'CurrentLevel': Status, 'ClaimID': ClaimID }),
        dataType: "json",
        success: function (response) {
            if (response.type == 1 || response.type == 2) {
                alert(response.ErrorMsg);
              
            }
                $("#loaderDiv").hide();
                $("#SubmitApprovalModal").modal("hide");
                window.location.href = response.Result;
            
        }
    });
}




//_NewClaim Functions

var table;
var cashPaid = 0;
var POItemsDetail = null;
POItemsDetail = [];

function parseMSDate(s) {
    var dregex = /\/Date\((\d*)\)\//;
    return dregex.test(s) ? new Date(parseInt(dregex.exec(s)[1])) : s;
}

function UpdatePOITEMSList(obj) {


    for (i = 0; i <= obj.length - 1; i++) {
        if ($('#CashPaid').val()) {
            cashPaid = $('#CashPaid').val();
        }

        cashPaid = parseFloat(cashPaid) + parseFloat(obj[i].GrossAmount);
        table.row.add([
            obj[i].Purchase_Order,
            obj[i].Doc_Co,
            obj[i].SupplierName,
            obj[i].InvoiceNumber,
            obj[i].InvoiceDate,
            obj[i].GrossAmount,

            "<input type='button' value='Remove' class='btnRemove btn btn-danger btn-xs'/>"
        ]).draw();
        POItemsDetail.push(obj[i]);


    }

    $('#CashPaid').val(cashPaid);

}


var ConfirmSubmit = function () {
    var x = confirm("Are you sure you want to Save?");

    if (x == false)
        return false;

    if ($('#cmbprojects').val() == '') {
        alert("Select Project");
        $('#cmbprojects').focus();
        return;
    }

    if ($('#cmbgroup').val() == '') {
        alert("Select Folder");
        $('#cmbgroup').focus();
        return;
    }

    if ($('#cmbgroup :selected').text() == "OTHERS") {
        if ($('#Remarks').val() == '') {
            alert("Enter Remarks ");
            $('#Remarks').focus();
            return;
        }
    }

    if ($('#Reason').val() == '') {
        alert("Enter Reason of Access ");
        $('#Reason').focus();
        return;
    }
    var mode = $('input[name=rdoAccess]:checked').val();
    if (typeof mode == 'undefined') {
        alert("Please Specify Required Access");
       // $('#rdoAccess').focus();
        return;
    }
  
  
 
    var url = $('#newclaim').data('request-url');
    var claim = new Object(); //Javascript Object

    

    claim.Group_Name = $('#cmbgroup :selected').text();
    claim.ProcessOwner = $('#cmbgroup').val();
    claim.RequiredAccess = $('input[name=rdoAccess]:checked').val();
    claim.Remarks = $('#Remarks').val();
    claim.Reason = $('#Reason').val();
    
    claim.EmpCode = $('#EmpCode').val()



    $.ajax
        ({
            type: "POST",
          
            url: url,
            data: JSON.stringify({ 'ClaimData': claim }),
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (data) {
              
                if (data.Response != '') {
                  
                    $('#message').html(data.Response);
                    return;
                }
                
                else {
                   
                    $('#ClaimitemDetail').html(data.PartialView);
                    $('#message').html("Saved Successfully...");
                    $('#Remarks').val('');
                    $('#Reason').val('');
                    $('input:radio').attr("checked",false);
                    

                }

            }
        });
        

}


var ConfirmSubmitService = function () {
    var x = confirm("Are you sure you want to Save?");

    if (x == false)
        return false;

    if ($('#cmbServices').val() == '') {
        alert("Select Service");
        $('#cmbServices').focus();
        return;
    }

    if ($('#Remarks').val() == '') {
        alert("Enter Justification ");
        $('#Remarks').focus();
        return;
    }



    if ($('#cmbServices').val() == 'SD_SUA' || $('#cmbServices').val() == 'SD_SFC' || $('#cmbServices').val() == 'SD_DBR' || $('#cmbServices').val() == "SD_Soft" || $('#cmbServices').val() == "SD_Misc" ) {
        if ($('#Path').val() == '') {
            alert("Specify Path / Aplication Name ");
            $('#Path').focus();
            return;
        }

    }

       

    var url = $('#newclaim').data('request-url');
    var claim = new Object(); //Javascript Object



    claim.ServiceName = $('#cmbServices :selected').text();
    claim.ServiceCode = $('#cmbServices').val();
   
    claim.Remarks = $('#Remarks').val();

    claim.Path = $('#Path').val();
    claim.EmpCode = $('#EmpCode').val()



    $.ajax
        ({
            type: "POST",

            url: url,
            data: JSON.stringify({ 'ClaimData': claim }),
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (data) {

                if (data.Response != '') {

                    $('#message').html(data.Response);
                    return;
                }

                else {
                    //alert(data.PartialView);
                    $('#ServiceitemDetail').html(data.PartialView);
                    $('#message').html("Saved Successfully...");
                    $('#Remarks').val('');
                    $('#Path').val('');
                    $('#cmbServices').focus();


                }

            }
        });


}

