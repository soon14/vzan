
function ajaxOper(url, type, data, dataType, success) {
    $.ajax({
        url: url,
        type: type,
        data: data,
        dataType:dataType,
        success: success,
    });
}
