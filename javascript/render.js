const concat = (xs, ys) => xs.concat(ys);

function randomNumber(range) {
    return Math.random() * (range[1] - range[0]) + range[0];
}

class Complex {
    constructor (re, im) {
        this.re = re;
        this.im = im;
    }

    static random(realRange, imagRange) {
        return new Complex(randomNumber(realRange), randomNumber(imagRange));
    }

    static zero() {return new Complex(0.0, 0.0);}

    static sub(u, v) {return new Complex(u.re - v.re, u.im - v.im);}

    static subFloat(u, v) {return new Complex(u.re - v, u.im - v);}

    static add(u, v) {return new Complex(u.re + v.re, u.im + v.im);}

    static addFloat(u, v) {return new Complex(u.re + v, u.im - v);}

    static zSquaredPlusC(u, v) {
        return new Complex(u.re**2 - u.im**2 + v.re, 2 * u.re * u.im + v.im);
    }

    abs() {return Math.sqrt(this.re**2 + this.im**2)}

    theta() {return Math.atan2(this.im, this.re);}

    rePositive() {return new Complex(Math.abs(this.re), this.im);}
}


class Buddhabrot {
    constructor (w, real, imag, iterations, p) {
        this.w = w;
        this.h = function() {return this.w * Math.abs((this.imag[1] - this.imag[0])/(this.real[1] - this.real[0]));}
        this.real = real;
        this.imag = imag;
        this.iterations = iterations;
        this.p = p;
        this.numbers;
        this.arr;
    };

    static linMap(x, from, to) {
        return to[0] + (x - from[0]) * (to[1] - to[0]) / (from[1] - from[0]);
    }

    static normalize2RGBA(arr, depth) {
        let from = [Math.min(...arr), Math.max(...arr)];
        let to = [0, 2**depth - 1];
        return Array.from(arr, i => {
            let a = Array(3).fill(Math.round(Buddhabrot.linMap(i, from, to)));
            a.push(255);
            return a;
        });
    }

    static p_1(z) {
        let c = Complex.subFloat(z, 0.25);
        return c.abs() < 0.5 * (1 - c.theta());
    };
    
    static p_2(z) {
        return Complex.addFloat(z, 1).abs() < 0.25;
    }

    static p_3(z) {
        return Complex.add(z.rePositive(), new Complex(0.125, -0.7432)).abs() < 0.094;
    }

    static p_4_2(z) {
        return Complex.addFloat(z, 1.309).abs() < 0.058;
    }

    static f(z, c) {
        return Complex.zSquaredPlusC(z, c);
    }

    complexToXY(z, h) {
        return [Math.round(Buddhabrot.linMap(z.re, this.real, [0, this.w])),
                Math.round(Buddhabrot.linMap(z.im, this.imag, [0, h]))];
    }

    inView(x, y, h) {return 0 <= x && x < this.w && 0 <= y && y < h}

    getNumbers() {
        this.numbers = [];
        for (let i = 0; i < this.p; i++)
        {   
            let z;
            //do {
            z = Complex.random(this.real, this.imag);
            //} while (Buddhabrot.p_1(z) || Buddhabrot.p_2(z) || Buddhabrot.p_3(z) || Buddhabrot.p_4_2(z));
            this.numbers.push(z);
        }
        
    }

    calculate() {
        let h = this.h();
        let x;
        let y;
        if (this.arr === undefined) this.arr = Array.from(Array(h), () => new Array(this.w).fill(0));
        for (let c of this.numbers) {
            let z = Complex.zero();
            let numbers = []
            for (let i = 0; i < this.iterations; i++) {
                z = Buddhabrot.f(z, c);
                [x, y] = this.complexToXY(z, h);
                if (this.inView(x, y, h)) {
                    numbers.push([x, y]);
                } else {
                    let i;
                    let j;
                    for ([i, j] of numbers) {
                        this.arr[j][i] += 1
                    }
                    break;
                }
            }
        }
    }

    show() {
        document.getElementById('buddha').src = this.toDataURL();
    }

    toDataURL(depth = 8) {
        let arr = Buddhabrot.normalize2RGBA(this.arr.flat(), depth).flat();
        const imgData = new ImageData(Uint8ClampedArray.from(arr), this.h(), this.w);
        var canvas = document.createElement("canvas");
        var ctx = canvas.getContext('2d');
        canvas.width = imgData.width;
        canvas.height = imgData.height;
        ctx.putImageData(imgData, 0, 0);
        return canvas.toDataURL();
    }
}

function getValue(id) {return document.getElementById(id).value;}

function getHeight() {
    return getValue('width') * 
    Math.abs((getValue('ymax') - getValue('ymin'))/
    (getValue('xmax') - getValue('xmin')));
}

function renderPressed() {
    document.getElementById('height').innerHTML = getHeight();
    let w = parseInt(getValue('width'));
    if (w > 350) alert("image might not be generated, try smaller value for width");
    let real = [parseFloat(getValue('xmin')), parseFloat(getValue('xmax'))];
    let imag = [parseFloat(getValue('ymin')), parseFloat(getValue('ymax'))];
    let iterations = parseInt(getValue('iterations'));
    let multiplier = parseFloat(getValue('mslider'));
    let buddha = new Buddhabrot(w, real, imag, iterations, Math.round(multiplier * w**2));
    buddha.getNumbers();
    buddha.calculate();
    buddha.show();
}