window.nutriDashboard = (() => {
    const charts = {};

    const colors = {
        ink: "#1F2A24",
        muted: "#5C665F",
        green: "#2F7D5B",
        greenDark: "#245A43",
        amber: "#E8B04B",
        amberDark: "#B5872B",
        terra: "#D96B52",
        teal: "#7BA7A3",
        pale: "rgba(31, 42, 36, 0.08)"
    };

    function destroy(canvasId) {
        if (charts[canvasId]) {
            charts[canvasId].destroy();
        }
    }

    function commonOptions(unitLabel) {
        return {
            responsive: true,
            maintainAspectRatio: false,
            animation: {
                duration: 900,
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
                        color: colors.ink,
                        font: {
                            family: "Inter",
                            weight: 600
                        }
                    }
                },
                tooltip: {
                    backgroundColor: colors.ink,
                    titleColor: "#FFFFFF",
                    bodyColor: "#F7F2E8",
                    padding: 12,
                    cornerRadius: 10,
                    callbacks: {
                        label: (context) => `${context.dataset.label}: ${context.parsed.y ?? context.parsed} ${unitLabel}`
                    }
                }
            },
            scales: {
                x: {
                    grid: {
                        display: false
                    },
                    ticks: {
                        color: colors.muted,
                        font: {
                            family: "Inter",
                            weight: 600
                        },
                        maxRotation: 0,
                        autoSkip: true
                    }
                },
                y: {
                    beginAtZero: true,
                    grid: {
                        color: colors.pale
                    },
                    ticks: {
                        color: colors.muted,
                        font: {
                            family: "Inter"
                        }
                    }
                }
            }
        };
    }

    function makeGradient(canvas, top, bottom) {
        const ctx = canvas.getContext("2d");
        const gradient = ctx.createLinearGradient(0, 0, 0, canvas.offsetHeight || 360);
        gradient.addColorStop(0, top);
        gradient.addColorStop(1, bottom);
        return gradient;
    }

    function renderBarChart(canvasId, labels, consumed, goals, unitLabel, consumedColor) {
        const canvas = document.getElementById(canvasId);
        if (!canvas || !window.Chart) {
            return;
        }

        destroy(canvasId);

        charts[canvasId] = new Chart(canvas.getContext("2d"), {
            type: "bar",
            data: {
                labels,
                datasets: [
                    {
                        label: "Recommended",
                        data: goals,
                        backgroundColor: "rgba(232, 176, 75, 0.34)",
                        borderColor: "rgba(181, 135, 43, 0.8)",
                        borderWidth: 1.5,
                        borderRadius: 8,
                        borderSkipped: false
                    },
                    {
                        label: "Hit so far",
                        data: consumed,
                        backgroundColor: makeGradient(canvas, consumedColor.top, consumedColor.bottom),
                        borderColor: consumedColor.border,
                        borderWidth: 1.5,
                        borderRadius: 8,
                        borderSkipped: false
                    }
                ]
            },
            options: commonOptions(unitLabel)
        });
    }

    function renderBalanceCharts(calorieCanvasId, macroCanvasId, calorieLabels, calorieConsumed, calorieGoals, macroLabels, macroConsumed, macroGoals) {
        renderBarChart(
            calorieCanvasId,
            calorieLabels,
            calorieConsumed,
            calorieGoals,
            "kcal",
            {
                top: "rgba(47, 125, 91, 0.95)",
                bottom: "rgba(47, 125, 91, 0.46)",
                border: "rgba(36, 90, 67, 0.92)"
            });

        renderBarChart(
            macroCanvasId,
            macroLabels,
            macroConsumed,
            macroGoals,
            "g",
            {
                top: "rgba(123, 167, 163, 0.96)",
                bottom: "rgba(217, 107, 82, 0.42)",
                border: "rgba(36, 90, 67, 0.72)"
            });
    }

    function lineDataset(label, data, color, dashed = false) {
        return {
            label,
            data,
            borderColor: color,
            backgroundColor: color,
            borderWidth: dashed ? 2 : 3,
            borderDash: dashed ? [7, 6] : [],
            pointRadius: 3,
            pointHoverRadius: 6,
            tension: 0.34
        };
    }

    function renderLineChart(canvasId, labels, datasets, unitLabel) {
        const canvas = document.getElementById(canvasId);
        if (!canvas || !window.Chart) {
            return;
        }

        destroy(canvasId);

        charts[canvasId] = new Chart(canvas.getContext("2d"), {
            type: "line",
            data: {
                labels,
                datasets
            },
            options: commonOptions(unitLabel)
        });
    }

    function renderTrendCharts(calorieCanvasId, macroCanvasId, weightCanvasId, labels, calories, baselineCalories, protein, carbs, fat, fiber, weightLabels, weights) {
        renderLineChart(
            calorieCanvasId,
            labels,
            [
                lineDataset("Calories", calories, colors.green),
                lineDataset("Baseline calories", baselineCalories, colors.amberDark, true)
            ],
            "kcal");

        renderLineChart(
            macroCanvasId,
            labels,
            [
                lineDataset("Protein", protein, colors.greenDark),
                lineDataset("Carbs", carbs, colors.amberDark),
                lineDataset("Fat", fat, colors.teal),
                lineDataset("Fiber", fiber, colors.terra)
            ],
            "g");

        renderLineChart(
            weightCanvasId,
            weightLabels,
            [
                lineDataset("Weight", weights, colors.terra)
            ],
            "kg");
    }

    return { renderBalanceCharts, renderTrendCharts };
})();
