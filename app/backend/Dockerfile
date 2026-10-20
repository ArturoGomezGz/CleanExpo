# Una imagen por servicio: docker build --build-arg BIN=cloud-api|worker backend
FROM rust:1.97-slim AS build
WORKDIR /src
COPY Cargo.toml Cargo.lock rust-toolchain.toml seed-dev.sql ./
COPY migrations migrations
COPY crates crates
ARG BIN
RUN cargo build --release --locked -p ${BIN} && cp target/release/${BIN} /app

FROM debian:bookworm-slim
RUN useradd -r -u 10001 app && apt-get update && apt-get install -y --no-install-recommends ca-certificates \
    && rm -rf /var/lib/apt/lists/*
COPY --from=build /app /usr/local/bin/app
USER app
CMD ["app"]
