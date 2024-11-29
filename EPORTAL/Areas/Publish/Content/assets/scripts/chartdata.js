const getOrCreateLegendList = (chart, id) => {
    const legendContainer = document.getElementById(id);
    let listContainer = legendContainer.querySelector('ul');

    if (!listContainer) {
        listContainer = document.createElement('ul');
        listContainer.classList = "d-flex justify-content-between align-items-center flex-wrap w-100";

        legendContainer.appendChild(listContainer);
    }

    return listContainer;
};

const htmlLegendPlugin = {
    id: 'htmlLegend',
    afterUpdate(chart, args, options) {
        const ul = getOrCreateLegendList(chart, options.containerID);

        // Remove old legend items
        while (ul.firstChild) {
            ul.firstChild.remove();
        }

        // Reuse the built-in legendItems generator
        const items = chart.options.plugins.legend.labels.generateLabels(chart);

        items.forEach(item => {
            const li = document.createElement('li');
            li.classList = "d-flex w-50 align-items-center";

            li.onclick = () => {
                const { type } = chart.config;
                if (type === 'pie' || type === 'doughnut') {
                    chart.toggleDataVisibility(item.index);
                } else {
                    chart.setDatasetVisibility(item.datasetIndex, !chart.isDatasetVisible(item.datasetIndex));
                }
                chart.update();
            };

            // Color box
            const boxSpan = document.createElement('span');
            boxSpan.style.background = item.fillStyle;
            boxSpan.style.borderColor = item.strokeStyle;
            boxSpan.style.borderWidth = item.lineWidth + 'px';
            boxSpan.style.borderRadius = '50%';
            boxSpan.style.display = 'inline-block';
            boxSpan.style.height = '0.75rem';
            boxSpan.style.marginRight = '10px';
            boxSpan.style.width = '0.75rem';

            // Text
            const textContainer = document.createElement('span');
            textContainer.style.margin = 0;
            textContainer.style.padding = 0;
            textContainer.style.color = '#5C5C5C';
            textContainer.style.fontWeight = '600';
            textContainer.style.fontSize = '0.9rem';
            textContainer.style.textDecoration = item.hidden ? 'line-through' : '';

            const text = document.createTextNode(item.text);
            textContainer.appendChild(text);

            li.appendChild(boxSpan);
            li.appendChild(textContainer);
            ul.appendChild(li);
        });
    }
};

function getOpts(legionName, cutout = 0) {
    return {
        responsive: true,
        cutout: cutout,
        plugins: {
            htmlLegend: {
                containerID: legionName,
            },
            legend: {
                display: false,
                position: 'bottom'
            },
            title: {
                display: true,
                align: 'start',
                font: {
                    weight: 'bold',
                    size: 16,
                },
                text: '',
                padding: 0
            },
            datalabels: {
                align: 'end',
                anchor: 'end',
                color: function (context) {
                    return '#000';
                },
                font: function (context) {
                    var w = context.chart.width;
                    return {
                        size: w < 512 ? 12 : 14,
                        weight: 'bold',
                    };
                },
                formatter: function (value, context) {
                    return context.chart.data.datasets[0].data[context.dataIndex] + '%';
                }
            }
        }
    }
}

// Piechart
new Chart(document.getElementById('chart_region_1').getContext('2d'), {
    type: 'pie',
    plugins: [ChartDataLabels, htmlLegendPlugin],
    data: {
        labels: ['Miền bắc', 'Miền trung', 'Miền Nam', 'Khác'],
        datasets: [
            {
                data: [75, 15, 7, 3],
                backgroundColor: [
                    '#139FD4',
                    '#CC6633',
                    '#FFFF00',
                    '#483D8B',
                ],
                shadowOffsetX: 3,
                shadowOffsetY: 3,
                shadowBlur: 10,
                shadowColor: 'rgba(0, 0, 0, 0.5)',
            }
        ]
    },
    options: getOpts('legend_1'),
});

new Chart(document.getElementById('chart_region_2').getContext('2d'), {
    type: 'pie',
    plugins: [ChartDataLabels, htmlLegendPlugin],
    data: {
        labels: ['Miền bắc', 'Miền trung', 'Miền Nam', 'Khác'],
        datasets: [
            {
                data: [75, 15, 7, 3],
                backgroundColor: [
                    '#139FD4',
                    '#CC6633',
                    '#FFFF00',
                    '#483D8B',
                ],
            }
        ]
    },
    options: getOpts('legend_2'),
});

new Chart(document.getElementById('chart_region_3').getContext('2d'), {
    type: 'pie',
    plugins: [ChartDataLabels, htmlLegendPlugin],
    data: {
        labels: ['Miền bắc', 'Miền trung', 'Miền Nam', 'Khác'],
        datasets: [
            {
                data: [75, 15, 7, 3],
                backgroundColor: [
                    '#139FD4',
                    '#CC6633',
                    '#FFFF00',
                    '#483D8B',
                ],
            }
        ]
    },
    options: getOpts('legend_3'),
});

// Barchart
const htmlLegendBarPlugin = {
    id: 'htmlLegend',
    afterUpdate(chart, args, options) {
        const ul = getOrCreateLegendList(chart, options.containerID);

        // Remove old legend items
        while (ul.firstChild) {
            ul.firstChild.remove();
        }

        // Reuse the built-in legendItems generator
        const items = chart.options.plugins.legend.labels.generateLabels(chart);
        const imgs = ['./images/dashboard/SiteQLKH_color.svg', './images/dashboard/card_color.svg', './images/dashboard/shop_color.svg'];

        items.forEach((item, index) => {
            const li = document.createElement('li');
            li.classList = "d-flex w-100 align-items-center";

            li.onclick = () => {
                const { type } = chart.config;
                if (type === 'pie' || type === 'doughnut') {
                    chart.toggleDataVisibility(item.index);
                } else {
                    chart.setDatasetVisibility(item.datasetIndex, !chart.isDatasetVisible(item.datasetIndex));
                }
                chart.update();
            };

            // icon
            const img = document.createElement('img');
            img.src = imgs[index];

            const divImg = document.createElement('div');
            divImg.classList = "category_img d-flex align-items-center justify-content-center";
            divImg.appendChild(img);

            // Color box
            const boxSpan = document.createElement('span');
            boxSpan.style.background = item.fillStyle;
            boxSpan.style.borderColor = item.strokeStyle;
            boxSpan.style.borderWidth = item.lineWidth + 'px';
            boxSpan.style.display = 'inline-block';
            boxSpan.style.height = '0.75rem';
            boxSpan.style.marginRight = '10px';
            boxSpan.style.width = '5rem';

            // Text
            const textContainer = document.createElement('span');
            textContainer.style.marginTop = '.3rem';
            textContainer.style.padding = 0;
            textContainer.style.color = '#5C5C5C';
            textContainer.style.fontSize = '0.9rem';
            textContainer.style.textDecoration = item.hidden ? 'line-through' : '';

            const text = document.createTextNode(item.text);
            textContainer.appendChild(text);

            //div
            const divBox = document.createElement('div');
            divBox.classList = "d-flex flex-column";
            divBox.appendChild(boxSpan);
            divBox.appendChild(textContainer);

            li.appendChild(divImg);
            li.appendChild(divBox);
            ul.appendChild(li);
        });
    }
};

new Chart(document.getElementById('chart_category').getContext('2d'), {
    type: 'bar',
    responsive: true,
    plugins: [ChartDataLabels, htmlLegendBarPlugin],
    data: {
        labels: ['Công nghệ', 'An toàn môi trường', 'Công nghệ thông tin', 'Cơ', 'Điện', 'Xây dựng'],
        datasets: [{
            label: 'Đối tác',
            data: [150, 30, 50, 60, 10, 50],
            backgroundColor: '#0097D0',
            barPercentage: 0.9,
            categoryPercentage: 0.5,
            datalabels: {
                anchor: 'end',
                align: 'top'
            }
        },
        {
            label: 'Nhà cung cấp',
            data: [50, 60, 120, 90, 40, 20],
            backgroundColor: '#ED561B',
            barPercentage: 0.9,
            categoryPercentage: 0.5,
            datalabels: {
                anchor: 'end',
                align: 'top'
            }
        },
        {
            label: 'Khách hàng',
            data: [100, 70, 50, 40, 45, 60],
            backgroundColor: '#DDDF00',
            barPercentage: 0.9,
            categoryPercentage: 0.5,
            datalabels: {
                anchor: 'end',
                align: 'top'
            }
        }]
    },
    options: {
        responsive: true,
        scales: {
            x: {
                grid: {
                    display: false
                },
                ticks: {
                    color: '#0F296D',
                    font: {
                        weight: '600'
                    }
                },
            },
            y: {
                grid: {
                    display: false
                }
            },
        },
        plugins: {
            htmlLegend: {
                containerID: 'bar_category_legend',
            },
            legend: {
                position: 'left',
                display: false
            },
            title: {
                display: false,
            }
        },
    },
});

function getPlugin(percent) {
    return {
        id: 'my-doughnut-text-plugin',
        afterDraw: function (chart, option) {
            const canvasBounds = document.getElementById('chart_complain_1').getBoundingClientRect();
            chart.ctx.textBaseline = 'middle';
            chart.ctx.textAlign = 'center';
            chart.ctx.font = 'bold 1.25rem Arial';

            chart.ctx.fillStyle = "#0097D0";
            chart.ctx.fillText(percent, canvasBounds.width / 2, canvasBounds.height * 0.50);

            chart.ctx.font = 'normal .8rem Arial';
            chart.ctx.fillStyle = "#5C5C5C";
            chart.ctx.fillText("khiếu nại", canvasBounds.width / 2, canvasBounds.height * 0.60)
        }
    }
}

// Dônughtchart
new Chart(document.getElementById('chart_complain_1').getContext('2d'), {
    type: 'doughnut',
    plugins: [ChartDataLabels, htmlLegendPlugin, getPlugin(650)],
    data: {
        labels: ['Đã xử lý', 'Đã tiếp nhận'],
        datasets: [
            {
                data: [20, 80],
                backgroundColor: [
                    '#FF6600',
                    '#139FD4',
                ],
            }
        ]
    },
    options: getOpts('complain_1', '60%'),
});

new Chart(document.getElementById('chart_complain_2').getContext('2d'), {
    type: 'doughnut',
    plugins: [ChartDataLabels, htmlLegendPlugin, getPlugin(1500)],
    data: {
        labels: ['Đã xử lý', 'Đã tiếp nhận'],
        datasets: [
            {
                data: [25, 75],
                backgroundColor: [
                    '#FF6600',
                    '#139FD4',
                ],
            }
        ]
    },
    options: getOpts('complain_2', '60%'),
});

new Chart(document.getElementById('chart_complain_3').getContext('2d'), {
    type: 'doughnut',
    plugins: [ChartDataLabels, htmlLegendPlugin, getPlugin(4800)],
    data: {
        labels: ['Đã xử lý', 'Đã tiếp nhận'],
        datasets: [
            {
                data: [23, 72],
                backgroundColor: [
                    '#FF6600',
                    '#139FD4',
                ],
            }
        ]
    },
    options: getOpts('complain_3', '60%'),
});
