use rust_decimal::{Decimal, RoundingStrategy};

pub const IVA: Decimal = Decimal::from_parts(16, 0, 0, false, 2); // 0.16

pub fn r2(d: Decimal) -> Decimal {
    d.round_dp_with_strategy(2, RoundingStrategy::MidpointAwayFromZero)
}

#[derive(Debug, Clone, Copy, Default, PartialEq)]
pub struct Totales {
    pub subtotal: Decimal,
    pub iva: Decimal,
    pub total: Decimal,
}

/// Mismo cálculo que `cart-math.ts` del frontend: subtotal e IVA se redondean por separado y luego se suman.
pub fn totales(lineas: impl IntoIterator<Item = (i32, Decimal, bool)>) -> Totales {
    let (mut subtotal, mut iva) = (Decimal::ZERO, Decimal::ZERO);
    for (cantidad, unitario, con_iva) in lineas {
        let importe = Decimal::from(cantidad) * unitario;
        subtotal += importe;
        if con_iva {
            iva += importe * IVA;
        }
    }
    let (subtotal, iva) = (r2(subtotal), r2(iva));
    Totales {
        subtotal,
        iva,
        total: subtotal + iva,
    }
}
