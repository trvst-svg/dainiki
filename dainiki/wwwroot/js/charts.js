(function () {
    "use strict";

    function hexToRgba(hex, alpha) {
        if (!hex) {
            return "rgba(0, 0, 0, " + alpha + ")";
        }

        var match = hex.trim().match(/^#?([a-f\\d]{2})([a-f\\d]{2})([a-f\\d]{2})$/i);
        if (!match) {
            return "rgba(0, 0, 0, " + alpha + ")";
        }

        var r = parseInt(match[1], 16);
        var g = parseInt(match[2], 16);
        var b = parseInt(match[3], 16);
        return "rgba(" + r + ", " + g + ", " + b + ", " + alpha + ")";
    }

    function getThemeVars() {
        var root = document.querySelector(".app-shell") || document.documentElement;
        var style = getComputedStyle(root);
        return {
            text: style.getPropertyValue("--text").trim() || "#222222",
            muted: style.getPropertyValue("--muted").trim() || "#666666",
            accent: style.getPropertyValue("--accent").trim() || "#d67a2c",
            accent2: style.getPropertyValue("--accent-2").trim() || "#2f8f7a",
            border: style.getPropertyValue("--border").trim() || "rgba(0,0,0,0.12)"
        };
    }

    function buildBaseOptions(theme) {
        return {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { display: false },
                tooltip: {
                    backgroundColor: theme.text,
                    titleColor: "#ffffff",
                    bodyColor: "#ffffff"
                }
            },
            scales: {
                x: {
                    grid: { display: false },
                    ticks: { color: theme.muted }
                },
                y: {
                    beginAtZero: true,
                    grid: { color: theme.border },
                    ticks: { color: theme.muted }
                }
            }
        };
    }

    window.dainikiCharts = {
        _charts: {},

        renderDashboard: function (consistencyId, wordId, labels, consistencyValues, wordValues) {
            if (!window.Chart) {
                return;
            }

            var theme = getThemeVars();
            Chart.defaults.color = theme.text;

            var consistencyCanvas = document.getElementById(consistencyId);
            var wordCanvas = document.getElementById(wordId);
            if (!consistencyCanvas && !wordCanvas) {
                return;
            }

            if (this._charts[consistencyId]) {
                this._charts[consistencyId].destroy();
                delete this._charts[consistencyId];
            }

            if (this._charts[wordId]) {
                this._charts[wordId].destroy();
                delete this._charts[wordId];
            }

            var baseOptions = buildBaseOptions(theme);

            if (consistencyCanvas) {
                this._charts[consistencyId] = new Chart(consistencyCanvas, {
                    type: "bar",
                    data: {
                        labels: labels,
                        datasets: [{
                            data: consistencyValues,
                            backgroundColor: hexToRgba(theme.accent, 0.85),
                            borderColor: theme.accent,
                            borderWidth: 1,
                            borderRadius: 6,
                            maxBarThickness: 26
                        }]
                    },
                    options: Object.assign({}, baseOptions, {
                        scales: {
                            x: baseOptions.scales.x,
                            y: Object.assign({}, baseOptions.scales.y, {
                                suggestedMax: 1,
                                ticks: Object.assign({}, baseOptions.scales.y.ticks, {
                                    stepSize: 1
                                })
                            })
                        }
                    })
                });
            }

            if (wordCanvas) {
                this._charts[wordId] = new Chart(wordCanvas, {
                    type: "line",
                    data: {
                        labels: labels,
                        datasets: [{
                            data: wordValues,
                            borderColor: theme.accent2,
                            backgroundColor: hexToRgba(theme.accent2, 0.2),
                            borderWidth: 2,
                            fill: true,
                            tension: 0.35,
                            pointRadius: 2
                        }]
                    },
                    options: baseOptions
                });
            }
        }
    };
})();
