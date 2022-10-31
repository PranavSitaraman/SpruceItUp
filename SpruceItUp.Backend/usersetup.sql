drop table users cascade;
create table if not exists users
(
    userid uuid not null constraint users_userid primary key,
    friendlyname varchar(255) not null,
    email varchar(255) not null constraint users_email unique,
    created timestamp not null,
    kind integer not null,
    signups uuid[] not null
);
alter table users owner to postgres;
drop table credentials cascade;
create table if not exists credentials
(
    credentialid uuid not null constraint credentials_credentialid primary key,
    userid uuid not null constraint credentials_userid references users,
    kind integer not null,
    identifier varchar(1023) not null,
    secret varchar(1023) not null
);
alter table credentials owner to postgres;
drop table pins cascade;
create table if not exists pins
(
    id uuid constraint pins_id primary key,
    author uuid not null constraint pins_author references users,
    title varchar(255) not null,
    lat double precision not null,
    lon double precision not null,
    kind integer not null,
    expires timestamp not null,
    created timestamp not null,
    image varchar(1023),
    description text
);
alter table pins owner to postgres;
drop table locs cascade;
create table if not exists locs
(
    id uuid constraint locs_id primary key,
    author uuid not null constraint locs_author references users,
    title varchar(255) not null,
    lat double precision[] not null,
    lng double precision[] not null,
    kind integer not null,
    expires timestamp not null,
    created timestamp not null,
    image varchar(1023),
    description text
);
alter table locs owner to postgres;
drop table comments cascade;
create table if not exists comments
(
    id uuid not null constraint comments_id primary key,
    author uuid not null constraint comments_author references users,
    pin uuid not null constraint comments_pin references pins,
    created timestamp not null,
    text text not null
);
alter table comments owner to postgres;