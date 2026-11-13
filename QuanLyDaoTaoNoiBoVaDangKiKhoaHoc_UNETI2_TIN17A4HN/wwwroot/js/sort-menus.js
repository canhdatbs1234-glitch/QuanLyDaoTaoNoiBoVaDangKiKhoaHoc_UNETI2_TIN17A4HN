// Định vị menu sắp xếp cạnh tiêu đề cột; Popover API giúp menu không bị cắt bởi bảng cuộn.
(() => {
    const placePopover = (popover) => {
        if (!(popover instanceof HTMLElement) || !popover.matches('.sort-popover') || !popover.matches(':popover-open')) return;
        const trigger = document.querySelector(`[popovertarget="${popover.id}"]`);
        if (!trigger) return;
        const rect = trigger.getBoundingClientRect();
        const margin = 8;
        const width = popover.offsetWidth || 220;
        const height = popover.offsetHeight || 180;
        const left = Math.max(margin, Math.min(rect.left, window.innerWidth - width - margin));
        let top = rect.bottom + 6;
        if (top + height > window.innerHeight - margin) top = Math.max(margin, rect.top - height - 6);
        popover.style.left = `${left}px`;
        popover.style.top = `${top}px`;
    };
    document.addEventListener('toggle', (event) => placePopover(event.target), true);
    window.addEventListener('resize', () => document.querySelectorAll('.sort-popover:popover-open').forEach(placePopover));
    window.addEventListener('scroll', () => document.querySelectorAll('.sort-popover:popover-open').forEach(placePopover), true);
})();
