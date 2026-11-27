FROM node:26
WORKDIR /src
COPY package*.json ./
RUN npm install

COPY . .

EXPOSE 4040

CMD ["npm", "run", "dev", "--", "--host", "0.0.0.0", "--port", "4040"]
