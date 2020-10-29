  
/**
   * Created by heart on 2017/4/26.
   */

(function () {
    var old_data = null;
    $(document).ready(function () {

        $("#select_room").css({'font-size':'18px'})
        startlist();
        clander_1();
        $('#demo').on('click', function () { $('#fReason').val('公司內部系統會議');$('#fReason').focusout()})
        $("#select_room").change(({ target: { value: v } }) => changeroom(v))
        $("#select_room").change(({ target: { value: v } }) => { startlist(v), $('#create-new').hide(); })
        $(".close").click(hidden);
        $("#changeop").click(changeop)
        $("#edit").on('mousedown', function (event) {
            if (event.target == event.currentTarget)
                hidden();
        });
        $("#seal").on('mousedown', function (event) {
            if (event.target == event.currentTarget)
                hidden();
        });
        //edit
        $("#fStarttime").change(({ target: { value: v } }) => {
            timelist(v, $('#fEndtime'));
            $('#fEndtime').val("請選時間");            
        });
        $("#fEndtime").change(({ target: { value: v } }) => {
            selectedTime(new Date(`2020-01-01 ${$("#fStarttime").val()}`), new Date(`2020-01-01 ${v}`));
            $("#fStarttime").focusout();
        }); 
        

        $("#fStarttime").attr('data-checktime', '');
        $("#fEndtime").attr('data-checktime', '');
        $("#fDate").attr('data-checkdate', '');
        $('#fDate').change(({ target: { value: v } }) => timelistcreate(v))
        //createshow
})
    function listclick(info) {  //按下日期
        var date = new Date(info).toLocaleDateString("fr-CA");
        $('#clickday').val(date);
        let R = $('#select_room').val();
        $.get(`/api/clickDayData/clickdaydata/${R}/${date}`, {}, function (data) {//搜尋按下日期的資料
            $('#list>h2').text(`搜尋日期:${date}`).css("text-align", 'left');
            $('#create-new').on('click', createshow)
            if (new Date(date).getDay() == 0 || new Date(date).getDay()== 6)//判斷按下是否為六日
                $('#create-new').hide();
            else if (((new Date(date).getMonth() == new Date().getMonth() && new Date(date).getDate() > new Date().getDate()))
                || (new Date(date).getMonth() > new Date().getMonth()))//按下日期大於當日
                $('#create-new').show();
            else if ((new Date(date).getDate() == new Date().getDate() && new Date(date).getMonth() == new Date().getMonth()) &&
                (new Date().getHours() < 17 || (new Date().getHours() == 16 && new Date().getMinutes() < 30)))//當日按下時間小於1630
                $('#create-new').show();            
            else
                $('#create-new').hide();

            let table = $('#home-table');
            table.show();
            table.DataTable().destroy();
            table = table[0];
            table.innerText = '';
            let theadRow = table.createTHead().insertRow(-1); //創建table <thead>
            theadRow.classList.add('bg-primary');               //加入class
            theadRow.insertCell(-1).innerText = '開會日期';
            theadRow.insertCell(-1).innerText = '借用人';
            theadRow.insertCell(-1).innerText = '開會原由';
            theadRow.insertCell(-1).innerText = '會議室';
            theadRow.insertCell(-1).innerText = '開始時間';
            theadRow.insertCell(-1).innerText = '結束時間';
            let tbodyRow = table.createTBody()              //創建table <tboday>
            for (let i of data) {
                let tr = tbodyRow.insertRow(-1); 
                tr.insertCell(-1).innerText = i.date;
                tr.insertCell(-1).innerText = i.Borrower;
                tr.insertCell(-1).innerText = i.Reason;
                tr.insertCell(-1).innerText = i.room + "會議室";
                tr.insertCell(-1).innerText = i.starttime.match(/^\d{1,2}:\d{1,2}/)[0];
                tr.insertCell(-1).innerText = i.endtime.match(/^\d{1,2}:\d{1,2}/)[0];
            }
            datatable();
            if (new Date(date).getDay() == 0 || new Date(date).getDay() == 6) {
                $('#list>h3').hide();
                $('#home-table').DataTable().destroy();
                $('#home-table').hide();
                $("#list>h2").text("假日無預約").css("text-align", 'center');
            }
        }, 'json');
    }

    function startlist() {          //登入者租借會議室列表
        let R = $('#select_room').val();
        let U = $('#userid').val();
        $.get(`/api/clickDayData//startlist/${R}/${U}`, {}, function (data) {
            let table = $('#home-table');
            table.DataTable().destroy();
            table = table[0];
            
            table.innerText = '';
            let theadRow = table.createTHead().insertRow(-1); //創建table <thead>
            theadRow.classList.add('bg-primary');           //加入class
            theadRow.insertCell(-1).innerText = '開會日期';
            theadRow.insertCell(-1).innerText = '開會原由';
            theadRow.insertCell(-1).innerText = '會議室';
            theadRow.insertCell(-1).innerText = '開始時間';
            theadRow.insertCell(-1).innerText = '結束時間';
            theadRow.insertCell(-1).innerText = '編輯/刪除';
            $('#list>h2').text("");
            if (data[0].date != null) {
                let tbodyRow = table.createTBody();         //創建table <tboday>
                for (let i of data) {
                    let tr = tbodyRow.insertRow(-1);
                    tr.insertCell(-1).innerText = i.date;
                    tr.insertCell(-1).innerText = i.reason;
                    tr.insertCell(-1).innerText = i.room + "會議室";
                    tr.insertCell(-1).innerText = i.starttime.match(/^\d{1,2}:\d{1,2}/)[0];
                    tr.insertCell(-1).innerText = i.endtime.match(/^\d{1,2}:\d{1,2}/)[0]
                    let cell = tr.insertCell(-1);
                    let edit = document.createElement('button');        //編輯按鈕
                    cell.appendChild(edit);
                    edit.innerText = '編輯';
                    edit.className = 'btn btn-success';
                    edit.onclick = function () {                
                        $.get(`/api/clickDayData/clickevent/${i.id}`, {}, function (data) {                            
                            editshow(data)                  //當行資料載入edit表單
                        });
                    };
                    let del = document.createElement('button');          //刪除按鈕   
                    cell.appendChild(del);
                    del.innerText = '刪除';
                    del.className = "btn btn-danger";
                    del.onclick = function () {
                        $.get(`/api/clickDayData/clickevent/${i.id}`, {}, function (data) {
                            delshow(data)                       //刪除確認載入當前fid
                        });
                    };
                    let seal = document.createElement('button');
                    cell.appendChild(seal)
                    seal.innerText = '通知';
                    seal.setAttribute('select-id',i.id)
                    seal.name = "sealbt";
                    seal.className = "btn btn-primary";
                    seal.onclick = function () {                        
                        sealbtn(i.id)
                    }
                    deledit();
                 }
            
            }
            datatable();
        }, 'json')
    }
    function sealbtn(id) {
        $.get(`/api/clickDayData/clickevent/${id}`, {}, function (data) {
            sealshow(data);               //當行資料載入edit表單
        });
    }
    function delshow(fid) {     //刪除確認
        Swal.fire({
            title: '您確定要撤銷嗎?',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: '確定'
        }).then((result) => {
            if (result.isConfirmed) {
                location.href = `/Metting_room/Delete?fId=${fid[0].fid}`
            }
        })
        //$("#del").fadeIn('slow');
        //$("#del-form").fadeIn('slow' )
        //$("#delb").click(function () {  })
    }
    function sealshow(data) {        
        $("#seal").fadeIn('slow');
        $("#seal-form").fadeIn('slow')
        depselect();
        $("#reason").text(`(${data[0].date})${data[0].reason}`)
        $("#time").text(`${data[0].starttime.match(/^\d{1,2}:\d{1,2}/)[0]}~${data[0].endtime.match(/^\d{1,2}:\d{1,2}/)[0]}`)

        $("#sealsend").click(function () {
            if ($("#department").val().length>0) {
                $.ajax({
                    type: "POST",
                    url: "/Metting_room/seal/",
                    data: {
                        empid: $("#department").val(),
                        reson: `(${data[0].date})${data[0].reason}`,
                        content: `${data[0].starttime.match(/^\d{1,2}:\d{1,2}/)[0]}~${data[0].endtime.match(/^\d{1,2}:\d{1,2}/)[0]}`
                    },
                    success: function (response) {
                        if (response == "source") {
                            Swal.fire({
                                position: 'center',
                                icon: 'success',
                                title: '通知成功',
                                showConfirmButton: false,
                                timer: 1500,
                                timerProgressBar: true,
                            })
                            hidden();
                        }
                    },
                    error: function (msg) {
                        //alert(msg.responseText);
                        Swal.fire({
                            icon: 'error',
                            title: '錯誤...',
                            text: '通知失敗',
                            footer: '',
                            timer: 1500,
                            timerProgressBar: true,
                        })
                    }
                })
            } else {
                Swal.fire({
                    icon: 'warning',
                    title: '警告',
                    text: '送出通知，人員不能為空值',
                    footer: '',
                    timer: 1500,
                    timerProgressBar: true,
                })
            } 
            
        })
    };


    function deledit() {        //編輯刪除按鈕取消
        let now = new Date($.ajax({ async: false }).getResponseHeader("Date"))      //伺服器時間取得
        $('#list > .table tr').toArray().forEach(function (tr) {        //table<tr>陣列化
            var startTime = new Date(`${$('td:first-child', tr).text().trim()} ${$('td:nth-child(4)', tr).text().trim()}`);//會議開始時間取得
            if (new Date(now) > startTime) $('td:nth-child(6)', tr).html('');   //若小於當前時間按鈕刪除
        })
    };

    function changeroom(v) {//改變會議室
        let room = $('#select_room').val();
        var calendarEl = document.getElementById('calendar_1');
        $.get(`/api/clickDayData/start/${room}`, {}, function (data) {//取會議室的資料
            $("#room").val($("#select_room").val())
            calendar.removeAllEventSources();
            calendar.addEventSource({       //fullclender事件插入
                url: `/api/clickDayData/start/${room}`,
                type: 'get',
                textColor: 'white'
            });
        }, 'json');

    };

    function datatable() {  //datatable呈現方式
        $('#list > .table').DataTable({
            "paginate": true,
            "lengthMenu": [8, 10, 15],
            
            "order": [[0, "desc"]],
            "autoWidth":false, 
            "language": {
                "emptyTable": "沒有任何資料，請選日期來借取會議室"
            }
        });
    }

    var calendar = null;

    function clander_1() {  //fullclender創建
        var room = $('#select_room').val()
        var calendarEl = document.getElementById('calendar_1');
        calendar = new FullCalendar.Calendar(calendarEl, {
            initialView: 'dayGridMonth',
            locale: 'zh-TW',
            headerToolbar: {
                left: 'prev,next',
                center: 'title',
                right: 'dayGridMonth,timeGridWeek'
            },
            
            height: 600,
            buttonText: { today: '今天', month: '月', week: '周', listWeek: '列表' },
            fixedWeekCount: false,
            slotMinTime: "08:00:00",
            allDaySlot: false,
            slotEventOverlap: false,
            slotMaxTime: "17:00:00",
            slotDuration: "00:15",
            dayMaxEventRows: 'true',
            selectable: 'true',
            select: function (info) {
                listclick(info.startStr);
            },
            eventDidMount: function (info) {
                $(info.el).tooltip({                    
                    title: info.event.extendedProps.description,
                    placement: "top",
                    trigger: "hover",
                    container: "body"
                });
            },
            eventSources: [
                {
                    url: `/api/clickDayData/start/${room}`, // use the `url` property                    
                    textColor: 'white'
                }

            ],
            eventTimeFormat: { // like '14:30:00'
                hour: '2-digit',
                minute: '2-digit',
                hour12: false
            },
            eventClick: function (info) {
                if (new Date() < new Date(info.event.startStr)) {
                    if ($("#user").val() == info.event._def.extendedProps.description.split(":")[1]) {
                        $.get(`/api/clickDayData/clickevent/${info.event.id}`, {}, function (data) {
                            editshow(data);
                        });
                    }                    
                }
            }
        });

        calendar.render();
    }

    function hidden() {//表單隱藏
       
        $("#dialog-form").fadeOut('slow');
        $("#edit").fadeOut('slow');
        $("#del").fadeOut('slow');
        $("#del-form").fadeOut('slow')
        $("#fStarttime").val("");
        $("#fEndtime").val("");
        $("#fReason").val("");
        $("#fId").remove();
        $("#fRoom").val("");
        $("#fEmployeeId").val("");
        $("#fName").val("");
        $("#seal").fadeOut('slow');
        $("#seal-form").fadeOut('slow')
        $("#changeop").text('切換專案群組')
        $("#prt").text('(下拉式選單可點擊部門全選部門人員)')
        document.getElementById("fStarttime").setAttribute('selected-item', '');
        document.getElementById("fEndtime").setAttribute('selected-item', '');
        $('#editfrom').validator('destroy')
    }
    function editshow(data) { //編輯表單
        $("#title").text("變更會議時間")
        depselect();
        $("#editfrom").attr('action', '/Metting_room/edit');
        if (document.getElementById("fId") == null) {
            var fid = document.createElement('input');
            fid.type = 'hidden';
            fid.name = fid.id = "fId";
            document.getElementById("editfrom").appendChild(fid);
        }        
        $("#dialog-form").fadeIn('slow');
        $("#edit").fadeIn('slow');
        timelistcreate(data[0].date);
        document.getElementById("fStarttime").setAttribute('selected-item', data[0].starttime.match(/^\d{1,2}:\d{1,2}/));
        document.getElementById("fEndtime").setAttribute('selected-item', data[0].endtime.match(/^\d{1,2}:\d{1,2}/));
        document.getElementById("fReason").value = data[0].reason;
        if ((new Date().getHours() === 16 && new Date().getMinutes() < 30) || new Date().getHours() < 16)
            fDate.min = new Date().getFullYear().toString() + "-" + ('0' + (new Date().getMonth() + 1).toString()).slice(-2) + "-" + ('0' + (new Date().getDate()).toString()).slice(-2)
        else
            fDate.min = new Date().getFullYear().toString() + "-" + ('0' + (new Date().getMonth() + 1).toString()).slice(-2) + "-" + ('0' + (new Date().getDate() +1 ).toString()).slice(-2)
        $("#fDate").val(data[0].date);
        document.getElementById("fId").setAttribute('value', data[0].fid);
        $("#send").val("儲存")
        isrood();      
    }
    function isrood() { //表單驗證 
        $("#fDate").attr('required', 'required');
        $("#fDate").attr('data-pattern-error', '請輸入日期');
        $("#fReason").attr('required', 'required');
        $("#fReason").attr('data-error', '請輸入內容');
        $('#editfrom').validator({
            custom: {
                checktime(v) {
                    var fStarttime = new Date(`2020-01-01 ${$("#fStarttime").val()}`);
                    var fEndtime = new Date(`2020-01-01 ${$("#fEndtime").val()}`);
                    var eventid = $("#fId").val()
                    if (fStarttime.toString() == "Invalid Date" || fEndtime.toString() == "Invalid Date")
                        return "請選正確時間";
                    
                    for (let t of old_data) {
                        console.log(t.starttime, t.endtime);
                        if (
                            t.id != eventid &&
                            fStarttime < new Date(`2020-01-01 ${t.endtime}`) &&
                            fEndtime > new Date(`2020-01-01 ${t.starttime}`)
                        ) {
                            return '時間衝突';
                        }
                    }
                },
                checkdate(v) {
                    var clickdate = new Date($("#fDate").val()).getDay();
                    if (clickdate == 0 || clickdate == 6)
                    {
                        return "請勿選假日時間";
                    }
                }
            }            
        }).on('submit', function (e) {
            if (e.isDefaultPrevented())  // 未驗證通過 則不處理
                return;
            //e.preventDefault(); // 防止原始 form 提交表单
        });
    }
    function createshow() { //申請新會議室表單
        $("#title").text("申請會議室")
        $("#editfrom").attr('action', '/Metting_room/create')        
        $("#dialog-form").fadeIn('slow');
        $("#edit").fadeIn('slow');
        var date=$("#list>h2").text().split(":")[1]
        timelistcreate(date);
        $("#fDate").val(date)
        $("#fRoom").val($("#select_room").val());
        $("#fEmployeeId").val($("#userid").val());
        $("#fName").val($("#user").val());
        $("#send").val("創建");
        if ((new Date().getHours() === 16 && new Date().getMinutes() < 30) || new Date().getHours() < 16)
            fDate.min = new Date().getFullYear().toString() + "-" + ('0' + (new Date().getMonth() + 1).toString()).slice(-2) + "-" + ('0' + (new Date().getDate()).toString()).slice(-2)
        else
            fDate.min = new Date().getFullYear().toString() + "-" + ('0' + (new Date().getMonth() + 1).toString()).slice(-2) + "-" + ('0' + (new Date().getDate() + 1).toString()).slice(-2)
        isrood();
    }

    function timelist(startTime, endTimeStlecter) { //表單中時段表創建
        if (startTime == null) startTime = '23:59:59';
        let endtime = $('option', endTimeStlecter);

        startTime = new Date(`2020-01-01 ${startTime}`);

        for (let i = 1; i < endtime.length; i++) {
            if (new Date(`2020-01-01 ${endtime[i].value}`) <= startTime) endtime[i].style.display = 'none';
            else endtime[i].style.display = 'block';
        }
    }

    function clearSelfSelected(startTime, endTime) {    //消除當前按下編輯 時段表class
        for (let i = startTime; i < endTime; i.setTime(i.getTime() + 1000 * 60 * 30)) {
            $(`.bar > .box_list[time='${i.toTimeString().match(/^\d{1,2}:\d{1,2}/)[0]}']`).removeClass('used');
        }
    }

    function selectedTime(startTime, endTime) {     //當前所選時間時段表class
        let newTimes = [];
        for (let i = startTime; i < endTime; i.setTime(i.getTime() + 1000 * 60 * 30)) {
            newTimes.push(i.toTimeString().match(/^\d{1,2}:\d{1,2}/)[0]);
        }
        for (let box of document.querySelectorAll('.bar > .box_list')) {
            if (newTimes.includes(box.getAttribute('time'))) {
                box.classList.add('selected');
            } else {
                box.classList.remove('selected');
            }
        }
    }


    function updatetimelist(v) {    //時段表
        var p = new Promise((resolve, reject) => {
            $.get(`/api/clickDayData/changedaylist/${v}`, {}, function (data, result, status) {
                if (status.status != 200) {
                    reject(data);
                    return;
                }
                resolve(data);
            });
        });
        return p;
    }

    function oldata_data(v) {
        var r = $("#select_room").val()
        var p = new Promise((resolve, reject) => {
            $.get(`/api/clickDayData/clickDaydata/${r}/${v}`, {}, function (data, result, status) {
                if (status.status != 200) {
                    reject(data);
                    return;
                }
                resolve(data);
            });
        });
        return p;
    }

    async function timelistcreate(v) {  //時段表創建
        var opt = document.getElementById('fStarttime');
        var ope = document.getElementById('fEndtime');
        var s = []
        var a = await updatetimelist(v);
        let i = 0;
        old_data = await oldata_data(v)
        var tlis = document.getElementById('timelist');
        opt.innerText = ""
        ope.innerText = ""
        tlis.innerText = "";

        for (let item of a.starttimelist) {     //開始時間下拉式選單加入
            let op = document.createElement('option');
            op.innerText = item;
            if (new Date(`2020-01-01 ${item}`).getTime() == new Date(`2020-01-01 ${opt.getAttribute('selected-item')}`).getTime())
                op.setAttribute('selected', '');
            opt.appendChild(op);
            if (i++ == 0) continue;
            let div = document.createElement('div')
            let ditext = document.createElement('span')
            div.setAttribute('time', item)
            div.classList.add('box_list');
            tlis.appendChild(div);
            ditext.innerText = item;
            div.appendChild(ditext);

        }
        for (let item of a.endtimelist) {       //結束時間下拉式選單加入
            let op = document.createElement('option');
            op.innerText = item;
            if (new Date(`2020-01-01 ${item}`).getTime() == new Date(`2020-01-01 ${ope.getAttribute('selected-item')}`).getTime())
                op.setAttribute('selected', '');
            ope.appendChild(op);
        }
        for (let item of old_data)      //已用使間加入class
            for (let s = new Date(`2020-01-01 ${item.starttime}`); s < new Date(`2020-01-01 ${item.endtime}`); s.setTime(s.getTime() + 1000 * 60 * 30))
                $(`.bar > .box_list[time='${s.toTimeString().match(/^\d{1,2}:\d{1,2}/)[0]}']`).addClass('used')

        selectedTime(new Date(`2020-01-01 ${$("#fStarttime").val()}`), new Date(`2020-01-01 ${$('#fEndtime').val()}`)); //編輯所選時間
        timelist($('#fStarttime').val(), $('#fEndtime').val())      //所選時間
        clearSelfSelected(new Date(`2020-01-01 ${$("#fStarttime").val()}`), new Date(`2020-01-01 ${$('#fEndtime').val()}`)); //編輯當前已選時間

    }

    function depselect() {
        var depse = document.getElementById("department");
        depse.innerText = '';
        $.post("/api/clickDayData/getdep", {}, function (data) {
            console.log(data)
            var a = document.createElement('optgroup')
            a.label = "資訊部"
            var b = document.createElement('optgroup')
            b.label = "人資部"
            var c = document.createElement('optgroup')
            c.label = "設計部"
            depse.appendChild(a)
            depse.appendChild(b)
            depse.appendChild(c)
            for (let i of data) {
                let p = document.createElement('option');
                p.value = i.fid;
                p.text = i.name;
                if (a.label == i.dep)
                    a.appendChild(p)
                else if (b.label == i.dep)
                    b.appendChild(p)
                else
                    c.appendChild(p)
            }
            if (!$('#department').hasClass("select2-hidden-accessible")) {
                select()
            }
        })

    }
    function changeop() {
        var select = document.getElementById('department')

        if ($("#changeop").text() != "切換部門群組") {
            $("#changeop").text('切換部門群組')
            $("#prt").text('(下拉式選單可點擊部門名稱全選部門人員)')
            select.innerText = '';
            $.get("/api/clickDayData/getproject", {}, function (data) {
                for (let i of data) {
                    let optg = document.createElement('optgroup');
                    optg.label = i.proname;
                    optg.setAttribute('projid', i.proid);
                    select.appendChild(optg);
                }
                $.get("/api/clickDayData/getteammeber", {}, function (mem) {
                    for (let i of mem) {
                        let opt = document.createElement('option');
                        opt.value = i.empid;
                        opt.text = i.emp;
                        $(`optgroup[projid=${i.proid}]`).append(opt)
                    }
                })

            })
        } else {
            $("#changeop").text('切換專案群組')
            $("#prt").text('(下拉式選單可點擊專案名稱全選專案人員)')
            select.innerText = '';
            depselect()
        }       
        
    }
   

    function select() {
        $('#department').select2({
            width:'100%',
            placeholder: {
                id: '-1', // the value of the option
                text: '請選擇(可複選)...'
            }
        })
        $(document).on("click", ".select2-results__group", function () {
            var groupName = $(this).text()
            var options = $('#department option');


            $.each(options, function (key, value) {

                console.log($(value)[0].parentElement)
                if ($(value)[0].parentElement.label==groupName ) {
                    $(value).prop("selected", "selected");
                }

            });
            $("#department").trigger("change");
            $("#department").select2('close');
        })
    }
})()