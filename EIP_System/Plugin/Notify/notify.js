
//小鈴鐺刷新
function bell(list) {
    document.getElementById("bell-dropdown-item").innerHTML = "";
    if (list.length === 0) {
        document.getElementById("bell-dropdown-item").innerHTML =
            '<li class="nav-item">' +
            '<a class="dropdown-item" >' +
            '<span>' +
            '沒有訊息'+
           '</span>' +
            '</a >' +
            '</li >';
    }
    else {
        let listlength = 0
        if (list.length > 5)
            listlength = list.length - 5
        for (let i = list.length - 1; i >= listlength; i--) {
            ntime = Date.parse(new Date()) / 1000
            time = Date.parse(list[i].fTime) / 1000
            let limit = ntime - time
            let content = "";
            if (limit < 60) {
                content = "剛剛";
            }
            else if (limit >= 60 && limit < 3600) {
                content = Math.floor(limit / 60) + "分鐘前";
            }
            else if (limit >= 3600 &&limit < 86400) {
                content = Math.floor(limit / 3600) + "小時前";
            }
            else if (limit >= 86400 && limit < 2592000) {
                content = Math.floor(limit / 86400) + "天前";
            }
            else if (limit >= 2592000 && limit < 31104000) {
                content = Math.floor(limit / 2592000) + "個月前";
            }
            else {
                content = "許久前";
            }
            document.getElementById("bell-dropdown-item").innerHTML +=
                '<li class="nav-item" id="nav-item'+i+'">' +
                '<a class="dropdown-item" id="dropdown-item'+i+'" >' +
                '<span>' +
                '<span>' + list[i].fTitle + '</span>' +
                '<span class="time">' + content + '</span>' +
                '</span>' +
                '<span class="message">' + list[i].fContent + '</span>' +
                '</a >' +
                '</li >'
            if (list[i].fType === 0) {
                bellring += 1
                $("#nav-item" + i).css({ "background": "#f6c3a9" })
            }
        }
        
        document.getElementById("bell-dropdown-item").innerHTML +=
            '<li class="nav-item"><div class="text-center" ><a class="dropdown-item"><strong>查看所有通知</strong></a></div></li>'
        if (bellring !== 0) {
            $("#bg-red-counts").css({ "display": "inline-block" })
            document.getElementById("bg-red-counts").innerHTML = bellring
        }
        else
            $("#bg-red-counts").css({ "display": "none" })
    }
    for (let i = 0; i < list.length; i++) {
        $("#nav-item" + i).off;
        $("#nav-item" + i).mouseenter(function () {
            $("#nav-item" + i).css({ "background": "#f7f7f7" })
            if (list[i].fType === 0) {
                $.ajax({
                    url: '/Index/UpdateBell',
                    method: "post",
                    contentType: 'application/json',
                    data: JSON.stringify({ id: list[i].fId })
                }).done(function (data) {
                    bellring -= 1
                    document.getElementById("bg-red-counts").innerHTML = bellring
                    if (bellring === 0)
                        $("#bg-red-counts").css({ "display": "none" })
                })
            }
        })
    }
}

