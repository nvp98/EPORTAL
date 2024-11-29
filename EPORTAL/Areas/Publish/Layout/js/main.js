document.addEventListener('DOMContentLoaded', function () {
    var splide = new Splide('.splide', {
        arrows: false,
        type    : 'loop',
		autoplay: true,
		interval: 3000
    });
    splide.mount();
});

document.addEventListener('DOMContentLoaded', function () {
    var splide = new Splide('.s_journeys_HP', {
        arrows: false,
        type    : 'loop',
		autoplay: true,
		interval: 3000
    });
    splide.mount();
});
