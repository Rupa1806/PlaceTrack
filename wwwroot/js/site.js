/* PlaceTrack: small bits of interactivity. No libraries needed here. */
(function () {
    'use strict';

    // ---- Mobile menu ----
    var navToggle = document.querySelector('[data-nav-toggle]');
    var nav = document.querySelector('[data-nav]');
    if (navToggle && nav) {
        navToggle.addEventListener('click', function () {
            var open = nav.classList.toggle('open');
            navToggle.setAttribute('aria-expanded', open ? 'true' : 'false');
        });
    }

    // ---- Success messages fade away after 5 seconds ----
    document.querySelectorAll('[data-autohide]').forEach(function (el) {
        setTimeout(function () {
            el.style.opacity = '0';
            setTimeout(function () { el.remove(); }, 450);
        }, 5000);
    });

    // ---- Numbers count up on load ----
    var reduceMotion = window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    document.querySelectorAll('[data-count]').forEach(function (el) {
        var target = parseInt(el.getAttribute('data-count'), 10);
        if (isNaN(target) || reduceMotion || target === 0) return;
        var start = null;
        function step(ts) {
            if (start === null) start = ts;
            var progress = Math.min((ts - start) / 700, 1);
            el.textContent = Math.round(target * progress);
            if (progress < 1) requestAnimationFrame(step);
        }
        el.textContent = '0';
        requestAnimationFrame(step);
    });

    // ---- "Are you sure?" on risky forms ----
    document.querySelectorAll('form[data-confirm]').forEach(function (form) {
        form.addEventListener('submit', function (e) {
            if (!window.confirm(form.getAttribute('data-confirm'))) e.preventDefault();
        });
    });

    // ---- Selects that save as soon as you change them ----
    document.querySelectorAll('select[data-autosubmit]').forEach(function (sel) {
        sel.addEventListener('change', function () { sel.form.submit(); });
    });

    // ---- Show / hide password ----
    document.querySelectorAll('[data-toggle-password]').forEach(function (btn) {
        btn.addEventListener('click', function () {
            var input = btn.parentElement.querySelector('input');
            var show = input.type === 'password';
            input.type = show ? 'text' : 'password';
            btn.textContent = show ? 'Hide' : 'Show';
        });
    });

    // ---- Demo account buttons on the login page ----
    document.querySelectorAll('[data-fill-email]').forEach(function (btn) {
        btn.addEventListener('click', function () {
            var email = document.querySelector('input[name="Email"]');
            var pass = document.querySelector('input[name="Password"]');
            if (email) email.value = btn.getAttribute('data-fill-email');
            if (pass) pass.value = btn.getAttribute('data-fill-pass');
            if (pass) pass.focus();
        });
    });

    // ---- Drives page: live search + "only eligible" filter ----
    var search = document.getElementById('driveSearch');
    var only = document.getElementById('onlyEligible');
    var cards = document.querySelectorAll('[data-drive]');
    var countLabel = document.getElementById('driveCount');
    var noMatch = document.getElementById('noMatch');

    function filterDrives() {
        var text = (search ? search.value : '').trim().toLowerCase();
        var eligibleOnly = only && only.checked;
        var shown = 0;

        cards.forEach(function (card) {
            var okText = text === '' || card.getAttribute('data-search').indexOf(text) !== -1;
            var okElig = !eligibleOnly || card.getAttribute('data-eligible') === '1';
            var show = okText && okElig;
            card.hidden = !show;
            if (show) shown++;
        });

        if (countLabel) countLabel.textContent = shown + ' of ' + cards.length + ' shown';
        if (noMatch) noMatch.hidden = shown !== 0;
    }

    if (cards.length) {
        if (search) search.addEventListener('input', filterDrives);
        if (only) only.addEventListener('change', filterDrives);
        filterDrives();
    }

    // ---- Charts (Chart.js is loaded only on the admin dashboard) ----
    window.addEventListener('load', function () {
        var canvases = document.querySelectorAll('canvas[data-chart]');
        if (!canvases.length) return;

        if (typeof Chart === 'undefined') {
            canvases.forEach(function (c) {
                var note = document.createElement('p');
                note.className = 'muted small';
                note.textContent = 'The chart needs an internet connection to load. The numbers are still shown below.';
                c.parentElement.replaceWith(note);
            });
            return;
        }

        var palette = ['#2447f5', '#e29a00', '#0f9d58', '#d63a3a', '#6a5acd', '#0891b2', '#c026d3', '#64748b'];
        // Status colours match the badges: Applied blue, Shortlisted amber, Selected green, Rejected red.
        var statusColors = ['#2447f5', '#e29a00', '#0f9d58', '#d63a3a'];

        canvases.forEach(function (canvas) {
            var type = canvas.getAttribute('data-chart');
            var labels = JSON.parse(canvas.getAttribute('data-labels'));
            var values = JSON.parse(canvas.getAttribute('data-values'));

            if (type === 'doughnut') {
                new Chart(canvas, {
                    type: 'doughnut',
                    data: { labels: labels, datasets: [{ data: values, backgroundColor: statusColors, borderWidth: 2, borderColor: '#fff' }] },
                    options: { maintainAspectRatio: false, cutout: '62%', plugins: { legend: { display: false } } }
                });
            } else {
                var max = canvas.getAttribute('data-max');
                new Chart(canvas, {
                    type: 'bar',
                    data: {
                        labels: labels,
                        datasets: [{ label: canvas.getAttribute('data-label') || '', data: values, backgroundColor: palette[0], borderRadius: 6, maxBarThickness: 36 }]
                    },
                    options: {
                        maintainAspectRatio: false,
                        plugins: { legend: { display: false } },
                        scales: {
                            y: { beginAtZero: true, suggestedMax: max ? Number(max) : undefined, ticks: { precision: 0 }, grid: { color: '#eef1f6' } },
                            x: { grid: { display: false } }
                        }
                    }
                });
            }
        });
    });
})();
