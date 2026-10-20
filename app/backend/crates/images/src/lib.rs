//! Pipeline de imágenes: valida límites, redimensiona a variantes estándar y recodifica a WebP.
//!
//! Por ahora solo es la librería (sin servicio HTTP ni almacenamiento): el siguiente paso es
//! exponerla desde un worker que lea del bucket, y subir las variantes de vuelta.
//! La codificación WebP de `image` es sin pérdida; compresión con pérdida/AVIF queda pendiente.

use image::{imageops::FilterType, DynamicImage, ImageFormat, ImageReader};
use std::io::Cursor;

#[derive(Debug, Clone, Copy)]
pub struct Limits {
    pub max_bytes: usize,
    pub max_pixels: u64,
}

impl Default for Limits {
    fn default() -> Self {
        Self {
            max_bytes: 10 * 1024 * 1024,
            max_pixels: 40_000_000,
        }
    }
}

/// Lado máximo (px) de cada variante; se conserva la proporción y nunca se agranda.
pub const VARIANTS: [(&str, u32); 3] = [("thumb", 160), ("card", 480), ("detail", 1200)];

#[derive(Debug, thiserror::Error)]
pub enum ImageError {
    #[error("la imagen pesa {0} bytes y el máximo es {1}")]
    TooLarge(usize, usize),
    #[error("formato no soportado (solo JPEG, PNG o WebP)")]
    UnsupportedFormat,
    #[error("la imagen tiene {0} píxeles y el máximo es {1}")]
    TooManyPixels(u64, u64),
    #[error("imagen ilegible: {0}")]
    Decode(#[from] image::ImageError),
    #[error("imagen ilegible: {0}")]
    Io(#[from] std::io::Error),
}

#[derive(Debug)]
pub struct Variant {
    pub name: &'static str,
    pub width: u32,
    pub height: u32,
    pub webp: Vec<u8>,
}

pub fn process(bytes: &[u8], limits: Limits) -> Result<Vec<Variant>, ImageError> {
    if bytes.len() > limits.max_bytes {
        return Err(ImageError::TooLarge(bytes.len(), limits.max_bytes));
    }
    let reader = ImageReader::new(Cursor::new(bytes)).with_guessed_format()?;
    if !matches!(
        reader.format(),
        Some(ImageFormat::Jpeg | ImageFormat::Png | ImageFormat::WebP)
    ) {
        return Err(ImageError::UnsupportedFormat);
    }
    // Se revisa el tamaño en píxeles antes de decodificar para no reventar memoria con bombas de descompresión.
    let (w, h) = reader.into_dimensions()?;
    let pixels = u64::from(w) * u64::from(h);
    if pixels > limits.max_pixels {
        return Err(ImageError::TooManyPixels(pixels, limits.max_pixels));
    }
    let img = image::load_from_memory(bytes)?;
    VARIANTS
        .iter()
        .map(|&(name, side)| encode(&img, name, side))
        .collect()
}

fn encode(img: &DynamicImage, name: &'static str, side: u32) -> Result<Variant, ImageError> {
    let resized = if img.width().max(img.height()) > side {
        img.resize(side, side, FilterType::Lanczos3)
    } else {
        img.clone()
    };
    let mut out = Cursor::new(Vec::new());
    DynamicImage::ImageRgba8(resized.to_rgba8()).write_to(&mut out, ImageFormat::WebP)?;
    Ok(Variant {
        name,
        width: resized.width(),
        height: resized.height(),
        webp: out.into_inner(),
    })
}

#[cfg(test)]
mod tests {
    use super::*;

    fn png(w: u32, h: u32) -> Vec<u8> {
        let mut out = Cursor::new(Vec::new());
        DynamicImage::new_rgb8(w, h)
            .write_to(&mut out, ImageFormat::Png)
            .unwrap();
        out.into_inner()
    }

    #[test]
    fn genera_variantes_sin_agrandar() {
        let v = process(&png(2000, 1000), Limits::default()).unwrap();
        assert_eq!(
            v.iter()
                .map(|x| (x.name, x.width, x.height))
                .collect::<Vec<_>>(),
            vec![
                ("thumb", 160, 80),
                ("card", 480, 240),
                ("detail", 1200, 600)
            ]
        );
        let small = process(&png(100, 50), Limits::default()).unwrap();
        assert!(small.iter().all(|x| x.width == 100 && x.height == 50));
    }

    #[test]
    fn rechaza_grandes_y_no_imagenes() {
        let limits = Limits {
            max_bytes: 10,
            ..Limits::default()
        };
        assert!(matches!(
            process(&png(10, 10), limits),
            Err(ImageError::TooLarge(..))
        ));
        assert!(matches!(
            process(b"no soy imagen", Limits::default()),
            Err(ImageError::UnsupportedFormat)
        ));
        let limits = Limits {
            max_pixels: 100,
            ..Limits::default()
        };
        assert!(matches!(
            process(&png(20, 20), limits),
            Err(ImageError::TooManyPixels(..))
        ));
    }
}
