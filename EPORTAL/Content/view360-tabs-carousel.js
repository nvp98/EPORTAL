// v360-tabs-carousel: smooth scroll horizontal voi arrow auto-hide.
// Truoc day dung page-by-page logic dua tren data-visible. Doi sang scroll-based:
//   - Neu noi dung KHONG overflow -> an arrow ca 2 ben
//   - Neu overflow -> hien arrow, click scroll ngang 80% viewport, smooth
//   - Active tab tu scroll vao view khi page load
//
// Markup support:
//   <div data-v360-tabs class="v360-tabs-wrap">
//     <button class="v360-tabs-arrow v360-tabs-arrow--prev">...</button>
//     <nav class="v360-tabs|v360-sc-tabs|v360-sc-subtabs"> .v360-tab|.v360-sc-tab|.v360-sc-subtab </nav>
//     <button class="v360-tabs-arrow v360-tabs-arrow--next">...</button>
//   </div>
(function () {
    function init(wrap) {
        // Tim nav element - support multiple legacy/showcase class names
        var nav = wrap.querySelector('nav') ||
                  wrap.querySelector('.v360-sc-tabs') ||
                  wrap.querySelector('.v360-sc-subtabs') ||
                  wrap.querySelector('.v360-tabs');
        var btnPrev = wrap.querySelector('.v360-tabs-arrow--prev');
        var btnNext = wrap.querySelector('.v360-tabs-arrow--next');
        if (!nav || !btnPrev || !btnNext) return;

        function updateArrows() {
            var overflow = nav.scrollWidth - nav.clientWidth > 4;
            // An ca 2 arrow neu khong can scroll
            if (!overflow) {
                btnPrev.style.display = 'none';
                btnNext.style.display = 'none';
                wrap.classList.add('is-no-overflow');
                return;
            }
            wrap.classList.remove('is-no-overflow');
            btnPrev.style.display = '';
            btnNext.style.display = '';

            var atStart = nav.scrollLeft <= 4;
            var atEnd   = nav.scrollLeft + nav.clientWidth >= nav.scrollWidth - 4;
            btnPrev.classList.toggle('is-disabled', atStart);
            btnNext.classList.toggle('is-disabled', atEnd);
        }

        btnPrev.addEventListener('click', function (e) {
            e.preventDefault();
            nav.scrollBy({ left: -nav.clientWidth * 0.8, behavior: 'smooth' });
        });
        btnNext.addEventListener('click', function (e) {
            e.preventDefault();
            nav.scrollBy({ left: nav.clientWidth * 0.8, behavior: 'smooth' });
        });

        // Sync arrow state khi scroll (debounce nhe)
        var scrollTimer;
        nav.addEventListener('scroll', function () {
            clearTimeout(scrollTimer);
            scrollTimer = setTimeout(updateArrows, 50);
        });

        // Recompute khi resize (vd: open/close sidebar khac)
        var resizeTimer;
        window.addEventListener('resize', function () {
            clearTimeout(resizeTimer);
            resizeTimer = setTimeout(updateArrows, 100);
        });

        // Active tab tu scroll vao view (centered) khi page load
        var activeTab = nav.querySelector('.active');
        if (activeTab) {
            // Defer den next frame de browser tinh xong layout
            requestAnimationFrame(function () {
                var navRect = nav.getBoundingClientRect();
                var tabRect = activeTab.getBoundingClientRect();
                if (tabRect.left < navRect.left || tabRect.right > navRect.right) {
                    // scrollIntoView se anh huong window scroll - thay vi do, set scrollLeft truc tiep
                    var tabOffset = activeTab.offsetLeft;
                    var centered = tabOffset - (nav.clientWidth - activeTab.offsetWidth) / 2;
                    nav.scrollLeft = Math.max(0, centered);
                }
                updateArrows();
            });
        } else {
            updateArrows();
        }
    }

    function initAll() {
        // Khi page Index render -> add class de CSS kill body scroll + padding zero
        if (document.querySelector('.v360-list-wrap')) {
            document.body.classList.add('v360-list-page');
            document.documentElement.classList.add('v360-list-page');
        }
        var wraps = document.querySelectorAll('[data-v360-tabs]');
        wraps.forEach(init);
    }
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initAll);
    } else {
        initAll();
    }
})();
