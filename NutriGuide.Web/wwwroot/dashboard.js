window.nutriDashboard = (() => {
    const charts = {};

    function renderMacroChart(canvasId, labels, consumed, goals) {
        const canvas = document.getElementById(canvasId);
        if (!canvas || !window.Chart) {
            return;
        }

        if (charts[canvasId]) {
            charts[canvasId].destroy();
        }

        const ctx = canvas.getContext("2d");
        const gradient = ctx.createLinearGradient(0, 0, 0, canvas.offsetHeight || 360);
        gradient.addColorStop(0, "rgba(47, 125, 91, 0.92)");
        gradient.addColorStop(1, "rgba(47, 125, 91, 0.42)");

        charts[canvasId] = new Chart(ctx, {
            type: "bar",
            data: {
                labels,
                datasets: [
                    {
                        label: "Recommended",
                        data: goals,
                        backgroundColor: "rgba(232, 176, 75, 0.38)",
                        borderColor: "rgba(181, 135, 43, 0.8)",
                        borderWidth: 1.5,
                        borderRadius: 8,
                        borderSkipped: false
                    },
                    {
                        label: "Hit so far",
                        data: consumed,
                        backgroundColor: gradient,
                        borderColor: "rgba(36, 90, 67, 0.92)",
                        borderWidth: 1.5,
                        borderRadius: 8,
                        borderSkipped: false
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                animation: {
                    duration: 950,
                    easing: "easeOutQuart"
                },
                interaction: {
                    intersect: false,
                    mode: "index"
                },
                plugins: {
                    legend: {
                        labels: {
                            boxWidth: 12,
                            boxHeight: 12,
                            color: "#1F2A24",
                            font: {
                                family: "Inter",
                                weight: 600
                            }
                        }
                    },
                    tooltip: {
                        backgroundColor: "#1F2A24",
                        titleColor: "#FFFFFF",
                        bodyColor: "#F7F2E8",
                        padding: 12,
                        cornerRadius: 10
                    }
                },
                scales: {
                    x: {
                        grid: {
                            display: false
                        },
                        ticks: {
                            color: "#5C665F",
                            font: {
                                family: "Inter",
                                weight: 600
                            }
                        }
                    },
                    y: {
                        beginAtZero: true,
                        grid: {
                            color: "rgba(31, 42, 36, 0.08)"
                        },
                        ticks: {
                            color: "#5C665F",
                            font: {
                                family: "Inter"
                            }
                        }
                    }
                }
            }
        });
    }

    return { renderMacroChart };
})();
