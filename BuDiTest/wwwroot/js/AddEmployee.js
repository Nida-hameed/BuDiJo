"use strict"

const selectEl = document.querySelector("#DepartmentID");

selectEl.addEventListener('change', (e) => {
    $.ajax({
        url: '/Manager/GetManagers',
        method: 'GET',
        data: {
            DeptId: e.target.value
        },
        dataJson: 'JSON',
        success: function (res) {
            if (res !== "") {
                var select = document.getElementById("mySelect");
                select.innerHTML = '';
                res.map((r) => {
                    var option = document.createElement("option");
                    option.setAttribute("value", r.id);
                    option.setAttribute("class", "test");
                    option.setAttribute("data-image", r.picturePath);
                    option.innerText = r.fullName;
                    select.appendChild(option);
                })
            }
        },
        error: function (err) {
            console.log(err);
        }
    })
})

$(document).ready(function () {
    $('.select2').select2({
        templateResult: function (data) {
            if (!data.element) {
                return data.text;
            }
            var $element = $(data.element);
            var $wrapper = $('<span></span>');
            var $image = $('<img>');
            $image.attr('src', $element.data('image'));
            $image.attr('width', '30px');
            $image.attr('height', '30px');
            $wrapper.append($image);
            $wrapper.append(' ' + data.text);
            return $wrapper;
        },
        templateSelection: function (data) {
            if (!data.element) {
                return data.text;
            }
            var $element = $(data.element);
            var $wrapper = $('<span></span>');
            var $image = $('<img>');
            $image.attr('src', $element.data('image'));
            $image.attr('width', '30px');
            $image.attr('height', '30px');
            $wrapper.append($image);
            $wrapper.append(' ' + data.text);
            return $wrapper;
        }
    });
});