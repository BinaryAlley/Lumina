/**
 * Manages the full-screen image viewer overlay, used to display a single image or a small photo gallery.
 */
class ImageViewerService {
    constructor() {
        this.overlay = null;
        this.imageElement = null;
        this.captionElement = null;
        this.spinnerElement = null;
        this.prevButton = null;
        this.nextButton = null;
        this.items = [];
        this.currentIndex = 0;
        this.keydownHandler = null;
    }

    /**
     * Opens the viewer with the given images.
     * @param {Array<{src: string, title?: string}>|{src: string, title?: string}} items - The image(s) to display.
     */
    open(items) {
        this.items = Array.isArray(items) ? items : [items];
        if (this.items.length === 0)
            return;
        this.ensureOverlay();
        this.currentIndex = 0;
        this.show();
        this.showItem(this.currentIndex);
    }

    /**
     * Closes the viewer and releases the currently displayed image.
     */
    close() {
        if (!this.overlay)
            return;
        this.overlay.classList.remove('open');
        if (this.keydownHandler) {
            document.removeEventListener('keydown', this.keydownHandler);
            this.keydownHandler = null;
        }
        // stop the browser from finishing the load of the previous image in the background
        this.imageElement.removeAttribute('src');
        this.items = [];
    }

    /**
     * Displays the previous image of the gallery, when there is one.
     */
    previous() {
        if (this.currentIndex > 0)
            this.showItem(this.currentIndex - 1);
    }

    /**
     * Displays the next image of the gallery, when there is one.
     */
    next() {
        if (this.currentIndex < this.items.length - 1)
            this.showItem(this.currentIndex + 1);
    }

    /**
     * Builds the overlay and appends it to the document body the first time it is used.
     * @private
     */
    ensureOverlay() {
        if (this.overlay)
            return;

        this.overlay = document.createElement('div');
        this.overlay.className = 'image-viewer-overlay';
        this.overlay.innerHTML = `
            <button type="button" class="image-viewer-button image-viewer-close" aria-label="Close"></button>
            <button type="button" class="image-viewer-button image-viewer-prev" aria-label="Previous"></button>
            <div class="image-viewer-stage">
                <div class="image-viewer-spinner"></div>
                <img class="image-viewer-image" alt="" />
                <div class="image-viewer-caption"></div>
            </div>
            <button type="button" class="image-viewer-button image-viewer-next" aria-label="Next"></button>
        `;

        this.imageElement = this.overlay.querySelector('.image-viewer-image');
        this.captionElement = this.overlay.querySelector('.image-viewer-caption');
        this.spinnerElement = this.overlay.querySelector('.image-viewer-spinner');
        this.prevButton = this.overlay.querySelector('.image-viewer-prev');
        this.nextButton = this.overlay.querySelector('.image-viewer-next');

        // clicking the dimmed background closes the viewer, but not the image or the controls
        this.overlay.addEventListener('click', (e) => {
            if (e.target === this.overlay)
                this.close();
        });

        // reveal the image only after it finished loading, so the spinner is visible meanwhile
        const handleImageLoaded = () => {
            this.spinnerElement.style.display = 'none';
            this.imageElement.classList.add('loaded');
        };
        this.imageElement.addEventListener('load', handleImageLoaded);
        this.imageElement.addEventListener('error', handleImageLoaded);

        this.overlay.querySelector('.image-viewer-close').addEventListener('click', () => this.close());
        this.prevButton.addEventListener('click', (e) => {
            e.stopPropagation();
            this.previous();
        });
        this.nextButton.addEventListener('click', (e) => {
            e.stopPropagation();
            this.next();
        });

        document.body.appendChild(this.overlay);
    }

    /**
     * Shows the overlay and registers the keyboard navigation.
     * @private
     */
    show() {
        this.overlay.classList.add('open');
        this.keydownHandler = (e) => {
            if (e.key === 'Escape')
                this.close();
            else if (e.key === 'ArrowLeft')
                this.previous();
            else if (e.key === 'ArrowRight')
                this.next();
        };
        document.addEventListener('keydown', this.keydownHandler);
    }

    /**
     * Displays the image at the given index and updates the gallery controls.
     * @private
     * @param {number} index - The index of the image to display.
     */
    showItem(index) {
        if (index < 0 || index >= this.items.length)
            return;
        this.currentIndex = index;
        const item = this.items[index];

        this.spinnerElement.style.display = 'block';
        this.imageElement.classList.remove('loaded');
        this.imageElement.alt = item.title || '';
        this.imageElement.src = item.src;

        if (item.title) {
            this.captionElement.textContent = item.title;
            this.captionElement.style.display = 'block';
        }
        else {
            this.captionElement.textContent = '';
            this.captionElement.style.display = 'none';
        }

        const hasMultipleItems = this.items.length > 1;
        this.prevButton.style.display = hasMultipleItems ? 'flex' : 'none';
        this.nextButton.style.display = hasMultipleItems ? 'flex' : 'none';
        this.prevButton.disabled = index === 0;
        this.nextButton.disabled = index === this.items.length - 1;
    }
}

// create a global instance of the ImageViewerService
const imageViewer = new ImageViewerService();
