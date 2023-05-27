"use strict"

function markAsRead(notificationId) {
    $.post('/Home/MarkAsRead', { notificationId: parseInt(notificationId) })
        .done(function (data) {
            console.log("Notification marked as read");
            $("#notification-" + notificationId).remove();
        })
        .fail(function (error) {
            console.error("Failed to mark notification as read:", error);
        });
}

document.addEventListener("DOMContentLoaded", () => {
    const clockBtn = document.getElementById("clockBtn");
    const clockheading = document.getElementById("clock-heading");
    const timeDisplay = document.getElementById("timeDisplay");
    const ClockInTime = document.getElementById("ClockInTime");
    const ClockOutTime = document.getElementById("ClockOutTime");
    let isClockIn = true;

    const displayTime = () => {
        const date = new Date();
        const time = date.toLocaleTimeString();
        document.getElementById("time").textContent = time;
    };
    setInterval(displayTime, 1000);

    const SetTime = () => {
        const date = new Date();
        const time = date.toLocaleTimeString();
        return time;
    };

    const SetClockIn = () => {
        $.ajax({
            url: "/ClockEvents/ClockIn",
            method: "POST",
            data: {},
            dataType: "json",
            success: (res) => {
                if (res.success === false) {
                    alert('You already clocked in today!');
                    return;
                }
                ClockTime(ClockInTime, ClockOutTime, clockheading, clockBtn)
            },
            error: (err) => {
                console.log(err);
            },
        });
    };

    const SetClockOut = () => {
        $.ajax({
            url: "/ClockEvents/ClockOut",
            method: "POST",
            data: {},
            dataType: "json",
            success: (res) => {
                if (res.success === false) {
                    alert('You already clocked out today!');
                    return;
                }
                ClockTime(ClockInTime, ClockOutTime, clockheading, clockBtn);
            },
            error: (err) => {
                console.log(err);
            },
        });
    };

    const ClockTime = (ClockInTime, ClockOutTime, clockheading, clockBtn) => {
        $.ajax({
            url: "/ClockEvents/LatestClockTime",
            method: "GET",
            data: {},
            dataType: "json",
            success: (res) => {
                if (res.data !== "") {
                    if (res.data === "exists") {
                        ClockInTime.innerHTML = convertToAmPm(res.data.clockInTime);
                        ClockOutTime.innerHTML = convertToAmPm(res.data.clockOutTime);
                        clockheading.innerHTML = "Already Clocked In";
                        clockBtn.style.backgroundColor = "#FF7784";
                    } else {
                        if (res.data.clockInTime !== null && res.data.clockOutTime !== null) {
                            ClockInTime.innerHTML = convertToAmPm(res.data.clockInTime);
                            ClockOutTime.innerHTML = convertToAmPm(res.data.clockOutTime);
                            clockheading.innerHTML = "Clock In";
                            clockBtn.style.backgroundColor = "#5F97F8";
                            isClockIn = true;
                        } else if (res.data.clockInTime !== null) {
                            ClockInTime.innerHTML = convertToAmPm(res.data.clockInTime);
                            clockheading.innerHTML = "Clock Out";
                            clockBtn.style.backgroundColor = "#FF7784";
                            isClockIn = false;
                        } else if (res.data.clockOutTime !== null) {
                            ClockOutTime.innerHTML = convertToAmPm(res.data.clockOutTime);
                            clockheading.innerHTML = "Clock In";
                            clockBtn.style.backgroundColor = "#5F97F8";
                            isClockIn = true;
                        }
                    }
                } else {
                    ClockOutTime.innerHTML = "00:00:00";
                    clockheading.innerHTML = "Clock In";
                    clockBtn.style.backgroundColor = "#5F97F8";
                    isClockIn = true;
                }
            },
            error: (err) => {
                console.log(err);
            },
        });
    };

    clockBtn.addEventListener("click", () => {
        if (isClockIn) {
            isClockIn = false;
            SetClockIn();
        } else {
            isClockIn = true;
            SetClockOut();
        }
    });

    ClockTime(ClockInTime, ClockOutTime, clockheading, clockBtn);
});

const convertToAmPm = (dateString) => {
    const date = new Date(dateString);
    const hours = date.getHours();
    const minutes = date.getMinutes();
    const amPm = hours >= 12 ? "PM" : "AM";
    const formattedHours = hours % 12 || 12;
    const formattedMinutes = minutes < 10 ? `0${minutes}` : minutes;

    return `${formattedHours}:${formattedMinutes} ${amPm}`;
};