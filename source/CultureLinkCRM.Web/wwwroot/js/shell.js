(function () {
    "use strict";

    var COLLAPSE_KEY = "clcrm-sidebar-collapsed";
    var MOBILE_BREAKPOINT = 992;

    function isMobile() {
        return window.innerWidth < MOBILE_BREAKPOINT;
    }

    function toggleSidebar() {
        if (isMobile()) {
            document.body.classList.toggle("sidebar-mobile-open");
        } else {
            var collapsed = document.body.classList.toggle("sidebar-collapsed");
            window.localStorage.setItem(COLLAPSE_KEY, collapsed ? "1" : "0");
        }
    }

    function closeMobileSidebar() {
        document.body.classList.remove("sidebar-mobile-open");
    }

    document.addEventListener("DOMContentLoaded", function () {
        var toggleButtons = document.querySelectorAll("[data-sidebar-toggle]");
        toggleButtons.forEach(function (btn) {
            btn.addEventListener("click", toggleSidebar);
        });

        var backdrop = document.querySelector("[data-sidebar-backdrop]");
        if (backdrop) {
            backdrop.addEventListener("click", closeMobileSidebar);
        }

        window.addEventListener("resize", function () {
            if (!isMobile()) {
                closeMobileSidebar();
            }
        });
    });
})();
